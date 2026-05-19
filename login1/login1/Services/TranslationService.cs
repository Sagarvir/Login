using TranslationService.DTO.Translation;
using TranslationService.Repositories;
using TranslationService.Repositories.Interfaces;
using TranslationService.Services.Interfaces;

namespace TranslationService.Services
{
    public class TranslationService : ITranslationService
    {
        private readonly ITranslationRepository _repo;

        public TranslationService(ITranslationRepository repo)
        {
            _repo = repo;
        }

        public async Task<object> CreateKey(CreateKeyRequest request, string empId)
        {
            if (string.IsNullOrEmpty(empId))
                throw new Exception("Invalid token.");

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
            if (string.IsNullOrEmpty(empId))
                throw new Exception("Invalid token.");

            if (request.Keys == null || request.Keys.Count == 0)
                throw new Exception("At least one key is required.");

            var normalized = request.Keys.Select(k => new NormalizedKeyDto
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

            if (duplicates.Any())
                throw new Exception("Some keys already exist.");

            var keys = normalized.Select(k => new TranslationKey
            {
                KeyName = k.KeyName!,
                OriginalText = k.OriginalText!,
                ProjectId = k.ProjectId,
                CreatedByEmpId = empId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            }).ToList();

            await _repo.AddKeys(keys);

            return new
            {
                message = "Keys created successfully.",
                keyIds = keys.Select(k => k.Id)
            };
        }
        public async Task<string> UpsertTranslationsAsync(BulkTranslationRequest request, string empId)
        {
            if (request.Translations == null || request.Translations.Count == 0)
                throw new Exception("At least one translation is required.");

            var normalizedItems = request.Translations
                .Select(t => new
                {
                    t.KeyId,
                    LanguageCode = t.LanguageCode.Trim().ToUpper(),
                    t.Value
                })
                .Cast<dynamic>()
                .ToList();

            if (normalizedItems.Any(t => t.KeyId <= 0))
                throw new Exception("Invalid KeyId.");

            if (normalizedItems.Any(t => string.IsNullOrWhiteSpace(t.LanguageCode)))
                throw new Exception("LanguageCode required.");

            var keyIds = normalizedItems.Select(t => (int)t.KeyId).Distinct().ToList();

            var validKeys = await _repo.GetValidKeyIdsAsync(keyIds);

            if (validKeys.Count != keyIds.Count)
                throw new Exception("Some keys not found.");

            var existing = await _repo.GetExistingTranslationsAsync(keyIds);

            await _repo.UpsertBulkAsync(normalizedItems, existing, empId);

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

        public async Task DeleteKey(int id, string empId)
        {
            if (string.IsNullOrEmpty(empId))
            {
                throw new Exception("Invalid token.");
            }

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
    }
}
