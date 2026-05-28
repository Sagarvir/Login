using System.Text.Json;
using System.Xml.Linq;
using Microsoft.AspNetCore.Hosting;
using Translation.Contracts.DTO.Translation;
using Translation.DAO.Repositories.Interfaces;
using Translation.Models.Entities;
using Translation.Service.Interfaces;


namespace Translation.Service.Services
{
    public class TranslationService : ITranslationService
    {
        private readonly ITranslationRepository _repo;
        private readonly IWebHostEnvironment _env;

        public TranslationService(ITranslationRepository repo, IWebHostEnvironment env)
        {
            _repo = repo;
            _env = env;
        }

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
        public async Task<string> InsertTranslationsAsync(BulkTranslationRequest request, string empId)
        {
            if (request.Translations == null || request.Translations.Count == 0)
                throw new Exception("At least one translation is required.");
            if (request.Translations.Any(t => t.KeyId <= 0))
                throw new Exception("Invalid KeyId.");
            if (request.Translations.Any(t => string.IsNullOrWhiteSpace(t.LanguageCode)))
                throw new Exception("LanguageCode required.");
            var normalizedItems = request.Translations
                .Select(t => new BulkTranslationItem
                {
                    KeyId = t.KeyId,
                    LanguageCode = t.LanguageCode.Trim().ToUpper(),
                    Value = t.Value
                })
                .ToList();

            var keyIds = normalizedItems.Select(t => t.KeyId).Distinct().ToList();

            var validKeys = await _repo.GetValidKeyIdsAsync(keyIds);

            var invalidKeyIds = keyIds.Except(validKeys).ToList();
            var validKeySet = validKeys.ToHashSet();

            var validItems = normalizedItems
                .Where(t => validKeySet.Contains(t.KeyId))
                .ToList();

            if (validItems.Count == 0)
            {
                var invalidMessage = invalidKeyIds.Count > 0
                    ? $" Invalid KeyIds: {string.Join(", ", invalidKeyIds)}."
                    : string.Empty;
                return $"No valid translations to save.{invalidMessage}";
            }

            var existing = await _repo.GetExistingTranslationsAsync(validKeys);

            await _repo.InsertBulkAsync(validItems, existing, empId);

            if (invalidKeyIds.Count > 0)
            {
                return $"Translations saved successfully for valid keys. Invalid KeyIds: {string.Join(", ", invalidKeyIds)}.";
            }

            return "Translations saved successfully.";
        }

        public async Task<string> InsertTranslationAsync(AddTranslationRequest request, string empId)
        {
            if (request.KeyId <= 0)
                throw new Exception("Valid KeyId is required.");

            if (string.IsNullOrWhiteSpace(request.LanguageCode))
                throw new Exception("LanguageCode is required.");

            var language = request.LanguageCode.Trim().ToUpper();

            var keyExists = await _repo.TranslationKeyExistsAsync(request.KeyId);
            if (!keyExists)
                throw new Exception("Translation key not found.");

            var languageExists = await _repo.LanguageExistsAsync(language);
            if (!languageExists)
                throw new Exception($"Language '{request.LanguageCode}' is not supported. Supported languages: en, es, fr, de, ja");

            var existing = await _repo.GetTranslationValueAsync(request.KeyId, language);

            if (existing != null)
            {
                throw new Exception($"Translation already exists for KeyId {request.KeyId} and Language '{language}'.");
            }
            else
            {
                var translation = new TranslationValue
                {
                    TranslationKeyId = request.KeyId,
                    LanguageCode = language,
                    Value = request.Value,
                    UpdatedByEmpId = empId,
                    UpdatedAt = DateTime.UtcNow
                };

                await _repo.SaveTranslationAsync(translation);
            }

            return "Translation saved successfully.";
        }

        public async Task<object> GetTranslationAsync(int keyId, string languageCode)
        {
            if (keyId <= 0)
                throw new Exception("Valid KeyId is required.");

            if (string.IsNullOrWhiteSpace(languageCode))
                throw new Exception("LanguageCode is required.");

            var language = languageCode.Trim().ToUpper();

            var translation = await _repo.GetTranslationForUiAsync(keyId, language);

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

        public async Task<object> GetAllTranslationsAsync(int keyId)
        {
            if (keyId <= 0)
                throw new Exception("Valid keyId is required.");

            var translations = await _repo.GetTranslationsByKeyAsync(keyId);

            return translations.Select(t => new
            {
                t.LanguageCode,
                t.Value
            });
        }

        public async Task<List<TranslationKeyWithValueDto>> GetKeysWithTranslationsAsync(string languageCode)
        {
            if (string.IsNullOrWhiteSpace(languageCode))
                throw new Exception("LanguageCode is required.");

            var language = languageCode.Trim().ToUpper();

            return await _repo.GetKeysWithTranslationsAsync(language);
        }

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
                Message = "Translations published successfully"
            };
        }

    }
}
