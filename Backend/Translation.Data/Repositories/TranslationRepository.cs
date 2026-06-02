
using Microsoft.EntityFrameworkCore;
using Translation.Contracts.DTO.Translation;
using Translation.DAO.Data;
using Translation.DAO.Repositories.Interfaces;
using Translation.Models.Entities;

namespace Translation.DAO.Repositories
{
    // Data access implementation for translation keys and values.
    public class TranslationRepository : ITranslationRepository
    {
        private readonly AppDbContext _context;

        public TranslationRepository(AppDbContext context)
        {
            _context = context;
        }

        // Check if a translation key already exists for a given project.
        public async Task<bool> KeyExists(string keyName, int projectId)
        {
            return await _context.TranslationKeys
                .AnyAsync(k => k.KeyName == keyName
                            && k.ProjectId == projectId
                            && k.IsActive);
        }

        // Add a new translation key to the database.
        public async Task AddKey(TranslationKey key)
        {
            _context.TranslationKeys.Add(key);
            await _context.SaveChangesAsync();
        }

        // Add multiple translation keys to the database in a single operation.
        public async Task AddKeys(List<TranslationKey> keys)
        {
            _context.TranslationKeys.AddRange(keys);
            await _context.SaveChangesAsync();
        }

        // Retrieve all active translation keys from the database, ordered by creation date.
        public async Task<List<TranslationKey>> GetAllKeys()
        {
            return await _context.TranslationKeys
                .Where(k => k.IsActive)
                .OrderByDescending(k => k.CreatedAt)
                .ToListAsync();
        }

        // Retrieve existing translation keys based on a list of dynamic objects containing key names and project IDs.
        public async Task<List<(string KeyName, int ProjectId)>> GetExistingKeys(List<dynamic> keys)
        {
            var projectIds = keys.Select(k => (int)k.ProjectId).Distinct().ToList();
            var keyNames = keys.Select(k => (string)k.KeyName).Distinct().ToList();

            var existing = await _context.TranslationKeys
                .Where(k => k.IsActive &&
                            projectIds.Contains(k.ProjectId) &&
                            keyNames.Contains(k.KeyName))
                .Select(k => new { k.KeyName, k.ProjectId })
                .ToListAsync();

            return existing.Select(e => (e.KeyName, e.ProjectId)).ToList();
        }

        // Overload of GetExistingKeys that accepts a list of strongly-typed CreateKeyItem objects instead of dynamic objects.
        public async Task<List<(string KeyName, int ProjectId)>> GetExistingKeys(List<CreateKeyItem> keys)
        {
            var projectIds = keys.Select(k => k.ProjectId).Distinct().ToList();
            var keyNames = keys.Select(k => k.KeyName).Distinct().ToList();

            var existing = await _context.TranslationKeys
                .Where(k => k.IsActive &&
                            projectIds.Contains(k.ProjectId) &&
                            keyNames.Contains(k.KeyName))
                .Select(k => new { k.KeyName, k.ProjectId })
                .ToListAsync();

            return existing.Select(e => (e.KeyName, e.ProjectId)).ToList();
        }

        // Retrieve a list of valid translation key IDs based on a provided list of key IDs, ensuring that only active keys are returned.
        public async Task<List<int>> GetValidKeyIdsAsync(List<int> keyIds)
        {
            return await _context.TranslationKeys
                .Where(k => k.IsActive && keyIds.Contains(k.Id))
                .Select(k => k.Id)
                .ToListAsync();
        }

        // Retrieve existing translation values based on a list of translation key IDs, ensuring that only values associated with active keys are returned.
        public async Task<List<TranslationValue>> GetExistingTranslationsAsync(List<int> keyIds)
        {
            return await _context.TranslationValues
                .Where(t => keyIds.Contains(t.TranslationKeyId))
                .ToListAsync();
        }

        // Resolve a translation key id by key name, returning null when not found or inactive.
        public async Task<int?> GetKeyIdByNameAsync(string keyName)
        {
            return await _context.TranslationKeys
                .Where(k => k.IsActive && k.KeyName == keyName)
                .Select(k => (int?)k.Id)
                .FirstOrDefaultAsync();
        }

        // Check if a translation key exists and is active based on its ID.
        public async Task<bool> TranslationKeyExistsAsync(int keyId)
        {
            return await _context.TranslationKeys
                .AnyAsync(k => k.Id == keyId && k.IsActive);
        }

        // Check if a language exists in the database based on its code.
        public async Task<bool> LanguageExistsAsync(string languageCode)
        {
            return await _context.Languages
                .AnyAsync(l => l.Code == languageCode);
        }

        // Retrieve a specific translation value based on the translation key ID and language code.
        public async Task<TranslationValue?> GetTranslationValueAsync(int keyId, string languageCode)
        {
            return await _context.TranslationValues
                .FirstOrDefaultAsync(t => t.TranslationKeyId == keyId && t.LanguageCode == languageCode);
        }

        // Save a translation value to the database, adding it if it doesn't exist or updating it if it does.
        public async Task SaveTranslationAsync(TranslationValue translation)
        {
            _context.TranslationValues.Add(translation);
            await _context.SaveChangesAsync();
        }

        // Save all pending changes to the database.
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        // Retrieve a translation value for UI display based on the translation key ID and language code, returning null if no matching translation is found.
        public async Task<TranslationValue?> GetTranslationForUiAsync(int keyId, string languageCode)
        {
            return await _context.TranslationValues
                .Where(t => t.TranslationKeyId == keyId && t.LanguageCode == languageCode)
                .Select(t => new TranslationValue
                {
                    TranslationKeyId = t.TranslationKeyId,
                    LanguageCode = t.LanguageCode,
                    Value = t.Value
                })
                .FirstOrDefaultAsync();
        }

        // Retrieve all translation values associated with a specific translation key ID, returning a list of translation values for UI display.
        public async Task<List<TranslationValue>> GetTranslationsByKeyAsync(int keyId)
        {
            return await _context.TranslationValues
                .Where(t => t.TranslationKeyId == keyId)
                .Select(t => new TranslationValue
                {
                    LanguageCode = t.LanguageCode,
                    Value = t.Value
                })
                .ToListAsync();
        }

        // Retrieve a list of translation keys along with their corresponding translation values for a specific language code, returning a list of DTOs that include the key information and the translated value.
        public async Task<List<TranslationKeyWithValueDto>> GetKeysWithTranslationsAsync(string languageCode)
        {
            return await _context.TranslationKeys
                .Where(k => k.IsActive)
                .Select(k => new TranslationKeyWithValueDto
                {
                    KeyId = k.Id,
                    Key = k.KeyName,
                    OriginalText = k.OriginalText,
                    ProjectId = k.ProjectId,
                    Value = k.Translations
                        .Where(t => t.LanguageCode == languageCode)
                        .Select(t => t.Value)
                        .FirstOrDefault() ?? ""
                })
                .ToListAsync();
        }

        // Insert or update translation values in bulk based on a list of items, checking for existing translations and updating them if found, or adding new translations if not found.
        public async Task InsertBulkAsync(
            List<BulkTranslationItem> items,
            List<TranslationValue> existing,
            string empId)
        {
            foreach (var item in items)
            {
                var found = existing.FirstOrDefault(t =>
                    t.TranslationKeyId == item.KeyId &&
                    t.LanguageCode == item.LanguageCode);

                if (found != null)
                {
                    //found.Value = item.Value;
                    //found.UpdatedByEmpId = empId;
                    //found.UpdatedAt = DateTime.UtcNow;
                    continue;
                }
                else
                {
                    _context.TranslationValues.Add(new TranslationValue
                    {
                        TranslationKeyId = item.KeyId,
                        LanguageCode = item.LanguageCode,
                        Value = item.Value,
                        UpdatedByEmpId = empId,
                        UpdatedAt = DateTime.UtcNow
                    });
                }
            }

            await _context.SaveChangesAsync();
        }

        // Retrieve a translation key along with its associated translations based on the key ID, returning null if no matching key is found or if the key is not active.
        public async Task<TranslationKey?> GetKeyByIdAsync(int id)
        {
            return await _context.TranslationKeys
                .Include(k => k.Translations)
                .FirstOrDefaultAsync(k => k.Id == id && k.IsActive);
        }

        // Soft delete a translation key by setting its IsActive property to false and saving the changes to the database.
        public async Task DeleteValuesAsync(List<TranslationValue> values)
        {
            if (values.Count == 0)
            {
                return;
            }

            _context.TranslationValues.RemoveRange(values);
            await _context.SaveChangesAsync();
        }

        // Soft delete a translation key by setting its IsActive property to false and saving the changes to the database.
        public async Task DeleteKeyAsync(TranslationKey key)
        {
            _context.TranslationKeys.Remove(key);
            await _context.SaveChangesAsync();
        }

        // Retrieve all translation values along with their associated languages and translation keys, returning a list of translation values for publishing purposes.
        public async Task<List<TranslationValue>> GetAllTranslationsForPublishAsync()
        {
            return await _context.TranslationValues
                .Include(tv => tv.Language)
                .Include(tv => tv.TranslationKey)
                .ToListAsync();
        }
        public async Task<List<TranslationValue>>GetTranslationsByLanguageAsync(string languageCode)
        {
            return await _context.TranslationValues

                .Include(x => x.TranslationKey)

                .Include(x => x.Language)

                .Where(x =>
                    x.LanguageCode.ToUpper()
                    ==
                    languageCode.ToUpper())

                .ToListAsync();
        }

        // Save a translation publish record to the database, adding it to the TranslationPublishes DbSet and saving the changes.
        public async Task SavePublishRecordAsync(TranslationPublish publishRecord)
        {
            _context.TranslationPublishes.Add(publishRecord);
            await _context.SaveChangesAsync();
        }
    }
}
