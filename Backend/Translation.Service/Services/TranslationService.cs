using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.IO.Compression;
using System.Text.Json;
using System.Xml.Linq;
using Translation.Contracts.DTO.Translation;
using Translation.DAO.Data;
using Translation.DAO.Repositories.Interfaces;
using Translation.Models.Entities;
using Translation.Service.Interfaces;
using TranslationService.DTO.Translation;


namespace Translation.Service.Services
{
    // Implements translation workflows and publishing logic.
    public class TranslationService : ITranslationService
    {
        private readonly ITranslationRepository _repo;
        private readonly IWebHostEnvironment _env;


        // Constructor injects dependencies for translation data access and environment info.
        

        public TranslationService(ITranslationRepository repo, IWebHostEnvironment env)

        {
            _repo = repo;
            _env = env;
        }

        // Creates a new translation key after validating input and checking for duplicates.
        public async Task<object> CreateKey(CreateKeyRequest request, string empId)
        {


            var keyName = request.KeyName.Trim().ToUpper();

            if (string.IsNullOrWhiteSpace(request.OriginalText))
                throw new Exception("Original text is required.");

            if (request.ProjectId <= 0)
                throw new Exception("Valid ProjectId is required.");

            var exists = await _repo.KeyExists(keyName, request.ProjectId);
            if (exists)
                throw new Exception("Key already exists in this project.");

            var key = new TranslationKey
            {
                KeyName = keyName,
                OriginalText = request.OriginalText,
                ProjectId = request.ProjectId,
                CreatedByEmpId = empId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _repo.AddKey(key);

            return new
            {
                message = "Key created successfully.",
                keyId = key.Id
            };
        }

        // Creates multiple translation keys in bulk, with validation and duplicate checking.
        public async Task<object> CreateKeys(CreateKeysRequest request, string empId)
        {

            if (request.Keys == null || request.Keys.Count == 0)
                throw new Exception("At least one key is required.");
          

            var normalized = request.Keys.Select(k => new CreateKeyItem
            {
                KeyName = k.KeyName?.Trim().ToUpper(),
                OriginalText = k.OriginalText?.Trim(),
                ProjectId = k.ProjectId
            }).ToList();

            if (normalized.Any(k => string.IsNullOrWhiteSpace(k.KeyName)))
                throw new Exception("KeyName is required.");

            if (normalized.Any(k => string.IsNullOrWhiteSpace(k.OriginalText)))
                throw new Exception("Original text is required.");

            if (normalized.Any(k => k.ProjectId <= 0))
                throw new Exception("Valid ProjectId required.");

            var duplicates = await _repo.GetExistingKeys(normalized);
            var existingSet = duplicates
                .Select(d => (d.KeyName.ToUpperInvariant(), d.ProjectId))
                .ToHashSet();

            var keys = normalized
                .Where(k => !existingSet.Contains((k.KeyName!, k.ProjectId)))
                .Select(k => new TranslationKey
                {
                    KeyName = k.KeyName!,
                    OriginalText = k.OriginalText!,
                    ProjectId = k.ProjectId,
                    CreatedByEmpId = empId,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                }).ToList();

            if (keys.Count == 0)
            {
                return new
                {
                    message = "No new keys to add.",
                    keyIds = Array.Empty<int>()
                };
            }

            await _repo.AddKeys(keys);

            return new
            {
                message = "Keys created successfully.",
                keyIds = keys.Select(k => k.Id)
            };
        }

        // Inserts or updates multiple translations in bulk, with validation and reporting of invalid keys.
        public async Task<string> InsertTranslationsAsync(BulkTranslationRequest request, string empId)
        {
            if (request.Translations == null || request.Translations.Count == 0)
                throw new Exception("At least one translation is required.");
            if (request.Translations.Any(t => string.IsNullOrWhiteSpace(t.KeyName)))
                throw new Exception("KeyName is required.");
            if (request.Translations.Any(t => string.IsNullOrWhiteSpace(t.LanguageCode)))
                throw new Exception("LanguageCode required.");

            var normalizedItems = request.Translations
                .Select(t => new
                {
                    KeyName = t.KeyName.Trim().ToUpperInvariant(),
                    LanguageCode = t.LanguageCode.Trim().ToUpperInvariant(),
                    t.Value
                })
                .ToList();

            var keyNames = normalizedItems.Select(t => t.KeyName).Distinct().ToList();
            var keyIdMap = new Dictionary<string, int>();

            foreach (var keyName in keyNames)
            {
                var keyId = await _repo.GetKeyIdByNameAsync(keyName);
                if (keyId.HasValue)
                {
                    keyIdMap[keyName] = keyId.Value;
                }
            }

            var invalidKeyNames = keyNames.Except(keyIdMap.Keys).ToList();
            var validItems = normalizedItems
                .Where(t => keyIdMap.ContainsKey(t.KeyName))
                .Select(t => new BulkTranslationItem
                {
                    KeyId = keyIdMap[t.KeyName],
                    LanguageCode = t.LanguageCode,
                    Value = t.Value
                })
                .ToList();

            if (validItems.Count == 0)
            {
                var invalidMessage = invalidKeyNames.Count > 0
                    ? $" Invalid KeyNames: {string.Join(", ", invalidKeyNames)}."
                    : string.Empty;
                return $"No valid translations to save.{invalidMessage}";
            }

            var validKeyIds = validItems.Select(t => t.KeyId).Distinct().ToList();
            var existing = await _repo.GetExistingTranslationsAsync(validKeyIds);

            await _repo.InsertBulkAsync(validItems, existing, empId);

            if (invalidKeyNames.Count > 0)
            {
                return $"Translations saved successfully for valid keys. Invalid KeyNames: {string.Join(", ", invalidKeyNames)}.";
            }

            return "Translations saved successfully.";
        }

        // Inserts a single translation after validating input and checking for existing translation.
        public async Task<string> InsertTranslationAsync(AddTranslationRequest request, string empId)
        {
            if (string.IsNullOrWhiteSpace(request.KeyName))
                throw new Exception("KeyName is required.");

            if (string.IsNullOrWhiteSpace(request.LanguageCode))
                throw new Exception("LanguageCode is required.");

            var language = request.LanguageCode.Trim().ToUpper();

            var keyName = request.KeyName.Trim().ToUpperInvariant();
            var keyId = await _repo.GetKeyIdByNameAsync(keyName);
            if (keyId == null)
                throw new Exception("Translation key not found.");

            var languageExists = await _repo.LanguageExistsAsync(language);
            if (!languageExists)
                throw new Exception($"Language '{request.LanguageCode}' is not supported. Supported languages: en, es, fr, de, ja");

            var existing = await _repo.GetTranslationValueAsync(keyId.Value, language);

            if (existing != null)
            {
                throw new Exception($"Translation already exists for KeyName {keyName} and Language '{language}'.");
            }
            else
            {
                var translation = new TranslationValue
                {
                    TranslationKeyId = keyId.Value,
                    LanguageCode = language,
                    Value = request.Value,
                    UpdatedByEmpId = empId,
                    UpdatedAt = DateTime.UtcNow
                };

                await _repo.SaveTranslationAsync(translation);
            }

            return "Translation saved successfully.";
        }

        // Retrieves a specific translation for a given key and language, returning an empty value if not found.
        public async Task<object> GetTranslationAsync(string key_name, string languageCode)
        {
            if (string.IsNullOrWhiteSpace(key_name))
                throw new Exception("Valid KeyName is required.");

            if (string.IsNullOrWhiteSpace(languageCode))
                throw new Exception("LanguageCode is required.");

            var language = languageCode.Trim().ToUpper();

            var keyId = await _repo.GetKeyIdByNameAsync(key_name);
            if (keyId == null)
                throw new Exception("Translation key not found.");

            var translation = await _repo.GetTranslationForUiAsync(keyId.Value, language);

            if (translation == null)
            {
                return new { value = "" };
            }

            return new
            {
                translation.TranslationKeyId,
                translation.LanguageCode,
                translation.Value
            };
        }

        // Retrieves all translations for a given key, returning an empty list if none are found.
        public async Task<object> GetAllTranslationsAsync(string keyName)
        {
            var keyId = await _repo.GetKeyIdByNameAsync(keyName);
            if (keyId == null)
                throw new Exception("Translation key not found.");

            var translations = await _repo.GetTranslationsByKeyAsync((int)keyId);

            return translations.Select(t => new
            {
                t.LanguageCode,
                t.Value
            });
        }

        // Retrieves all translation keys along with their translations for a specific language.
        public async Task<List<TranslationKeyWithValueDto>> GetKeysWithTranslationsAsync(string languageCode)
        {
            if (string.IsNullOrWhiteSpace(languageCode))
                throw new Exception("LanguageCode is required.");

            var language = languageCode.Trim().ToUpper();

            return await _repo.GetKeysWithTranslationsAsync(language);
        }

        // Inserts or updates multiple translations in bulk, with validation and reporting of invalid keys (version 2).
        public async Task<string> UpsertTranslationsV2Async(BulkTranslationRequestV2 request, string empId)
        {
            if (request.Translations == null || request.Translations.Count == 0)
                throw new Exception("At least one translation is required.");

            

            if (request.Translations.Any(t => t.KeyId <= 0))
                throw new Exception("Invalid KeyId.");

            if (request.Translations.Any(t => string.IsNullOrWhiteSpace(t.LanguageCode)))
                throw new Exception("LanguageCode required.");

            var keyIds = request.Translations.Select(t => t.KeyId).Distinct().ToList();

            var validKeys = await _repo.GetValidKeyIdsAsync(keyIds);

            if (validKeys.Count != keyIds.Count)
                throw new Exception("Some keys not found.");

            var normalizedItems = request.Translations
                .Select(t => new BulkTranslationItem
                {
                    KeyId = t.KeyId,
                    LanguageCode = t.LanguageCode.Trim().ToUpper(),
                    Value = t.Value
                })
                .ToList();

            var existing = await _repo.GetExistingTranslationsAsync(keyIds);

            await _repo.InsertBulkAsync(normalizedItems, existing, empId);

            return "Translations saved successfully.";
        }

        // Retrieves all translation keys along with their original text and project ID.
        public async Task<object> GetAllKeys()
        {
            var keys = await _repo.GetAllKeys();

            return keys.Select(k => new
            {
                k.KeyName,
                k.OriginalText,
                k.ProjectId
            });
        }

        // Deletes a translation key and its associated translations after validating the key ID and checking for existence.
        public async Task DeleteKey(int id)
        {
            if (id <= 0)
            {
                throw new Exception("Invalid key id.");
            }

            var key = await _repo.GetKeyByIdAsync(id);

            if (key == null)
            {
                throw new Exception("Key not found.");
            }

            if (key.Translations.Count > 0)
            {
                await _repo.DeleteValuesAsync(key.Translations.ToList());
            }

            await _repo.DeleteKeyAsync(key);
        }

        // Publishes all translations by generating JSON and XLF files for each language, saving them to a versioned folder, and recording the publish event in the database.
        public async Task<PublishTranslationResponse> PublishTranslationsAsync()
        {
            var translations =
                await _repo.GetAllTranslationsForPublishAsync();

            if (!translations.Any())
            {
                return new PublishTranslationResponse
                {
                    Success = false,
                    Message = "No translations found"
                };
            }

            var version =
                $"v{DateTime.UtcNow:yyyyMMddHHmmss}";

            var publishFolder =
                Path.Combine(
                    _env.ContentRootPath,
                    "PublishedTranslations",
                    version
                );

            Directory.CreateDirectory(publishFolder);

            var groupedTranslations =
                translations.GroupBy(t => t.Language.Code);

            var fileCount = 0;

            foreach (var group in groupedTranslations)
            {
                var languageCode = group.Key;

                var jsonDictionary =
                    group.ToDictionary(
                        t => t.TranslationKey.KeyName,
                        t => t.Value
                    );

                var json =
                    JsonSerializer.Serialize(
                        jsonDictionary,
                        new JsonSerializerOptions
                        {
                            WriteIndented = true
                        });

                var jsonPath =
                    Path.Combine(
                        publishFolder,
                        $"{languageCode}.json"
                    );

                await File.WriteAllTextAsync(
                    jsonPath,
                    json);

                fileCount++;

                var xlf =
                    new XDocument(
                        new XElement("xliff",
                            new XAttribute("version", "1.2"),

                            new XElement("file",
                                new XAttribute(
                                    "source-language",
                                    "en-US"),

                                new XAttribute(
                                    "target-language",
                                    languageCode),

                                new XElement("body",

                                    group.Select(t =>

                                        new XElement("trans-unit",
                                            new XAttribute(
                                                "id",
                                                t.TranslationKey.KeyName),

                                            new XElement(
                                                "source",
                                                t.TranslationKey.KeyName),

                                            new XElement(
                                                "target",
                                                t.Value)
                                        )
                                    )
                                )
                            )
                        )
                    );

                var xlfPath =
                    Path.Combine(
                        publishFolder,
                        $"{languageCode}.xlf"
                    );

                xlf.Save(xlfPath);

                fileCount++;
            }

            var publishRecord =
                new TranslationPublish
                {
                    Version = version,
                    PublishedAt = DateTime.UtcNow,
                    PublishedBy = "Creator",
                    FileCount = fileCount
                };

            await _repo.SavePublishRecordAsync(publishRecord);

            return new PublishTranslationResponse
            {
                Success = true,
                Version = version,
                FileCount = fileCount,
                Message = publishFolder
            };
        }
        public async Task<PublishTranslationResponse>PublishLanguageAsync(string languageCode)
        {
            var translations =
                await _repo
                    .GetTranslationsByLanguageAsync(
                        languageCode);

            if (!translations.Any())
            {
                return new PublishTranslationResponse
                {
                    Success = false,
                    Message =
                        $"No translations found for {languageCode}"
                };
            }

            var version =
                $"v{DateTime.UtcNow:yyyyMMddHHmmss}";

            var publishFolder =
                Path.Combine(
                    _env.ContentRootPath,
                    "PublishedTranslations",
                    languageCode,
                    version
                );
            Console.WriteLine($"Publish Folder: {publishFolder}");

            Directory.CreateDirectory(
                publishFolder);

            var jsonDictionary =
                translations.ToDictionary(
                    t => t.TranslationKey!.KeyName,
                    t => t.Value
                );

            var json =
                JsonSerializer.Serialize(
                    jsonDictionary,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });

            var jsonPath =
                Path.Combine(
                    publishFolder,
                    $"{languageCode}.json");

            await File.WriteAllTextAsync(
                jsonPath,
                json);

            var xlf =
                new XDocument(
                    new XElement(
                        "xliff",
                        new XAttribute(
                            "version",
                            "1.2"),

                        new XElement(
                            "file",

                            new XAttribute(
                                "source-language",
                                "en"),

                            new XAttribute(
                                "target-language",
                                languageCode),

                            new XElement(
                                "body",

                                translations.Select(t =>

                                    new XElement(
                                        "trans-unit",

                                        new XAttribute(
                                            "id",
                                            t.TranslationKey!.Id),

                                        new XElement(
                                            "source",
                                            t.TranslationKey
                                             .OriginalText),

                                        new XElement(
                                            "target",
                                            t.Value)
                                    )
                                )
                            )
                        )
                    )
                );

            var xlfPath =
                Path.Combine(
                    publishFolder,
                    $"{languageCode}.xlf");

            xlf.Save(xlfPath);

            return new PublishTranslationResponse
            {
                Success = true,
                Version = version,
                FileCount = 2,
                Message = publishFolder
            };
        }
        public async Task<PublishDownloadResponse> PublishTranslationsAsZipAsync()
        {
            var publishResult = await PublishTranslationsAsync();

            if (!publishResult.Success)
            {
                return new PublishDownloadResponse
                {
                    Success = false,
                    Message = publishResult.Message
                };
            }

            var publishFolder = Path.Combine(
                _env.ContentRootPath,
                "PublishedTranslations",
                publishResult.Version
            );

            var zipFileName = $"PublishedTranslations_{publishResult.Version}.zip";

            using var memoryStream = new MemoryStream();

            ZipFile.CreateFromDirectory(
                publishFolder,
                memoryStream
            );

            return new PublishDownloadResponse
            {
                Success = true,
                Message = "Published and downloaded successfully",
                FileName = zipFileName,
                FileBytes = memoryStream.ToArray()
            };
        }

        public async Task<PublishDownloadResponse> PublishLanguageAsZipAsync(string languageCode)
        {
            var publishResult = await PublishLanguageAsync(languageCode);

            if (!publishResult.Success)
            {
                return new PublishDownloadResponse
                {
                    Success = false,
                    Message = publishResult.Message
                };
            }

            var publishFolder = Path.Combine(
                _env.ContentRootPath,
                "PublishedTranslations",
                languageCode,
                publishResult.Version
            );

            var zipFileName = $"{languageCode}_{publishResult.Version}.zip";

            using var memoryStream = new MemoryStream();

            ZipFile.CreateFromDirectory(
                publishFolder,
                memoryStream
            );

            return new PublishDownloadResponse
            {
                Success = true,
                Message = $"{languageCode} published and downloaded successfully",
                FileName = zipFileName,
                FileBytes = memoryStream.ToArray()
            };
        }


    }
}
