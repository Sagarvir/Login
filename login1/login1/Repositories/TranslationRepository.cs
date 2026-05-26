using login1.Data;
using login1.Models;
using TranslationService.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using TranslationService.DTO.Translation;
namespace TranslationService.Repositories
{
    public class TranslationRepository : ITranslationRepository
    {
        private readonly AppDbContext _context;

        public TranslationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> KeyExists(string keyName, int projectId)
        {
            return await _context.TranslationKeys
                .AnyAsync(k => k.KeyName == keyName
                            && k.ProjectId == projectId
                            && k.IsActive);
        }

        public async Task AddKey(TranslationKey key)
        {
            _context.TranslationKeys.Add(key);
            await _context.SaveChangesAsync();
        }

        public async Task AddKeys(List<TranslationKey> keys)
        {
            _context.TranslationKeys.AddRange(keys);
            await _context.SaveChangesAsync();
        }

        public async Task<List<TranslationKey>> GetAllKeys()
        {
            return await _context.TranslationKeys
                .Where(k => k.IsActive)
                .OrderByDescending(k => k.CreatedAt)
                .ToListAsync();
        }

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

        public async Task<List<(string KeyName, int ProjectId)>> GetExistingKeys(List<NormalizedKeyDto> keys)
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
        public async Task<List<int>> GetValidKeyIdsAsync(List<int> keyIds)
        {
            return await _context.TranslationKeys
                .Where(k => k.IsActive && keyIds.Contains(k.Id))
                .Select(k => k.Id)
                .ToListAsync();
        }

        public async Task<List<TranslationValue>> GetExistingTranslationsAsync(List<int> keyIds)
        {
            return await _context.TranslationValues
                .Where(t => keyIds.Contains(t.TranslationKeyId))
                .ToListAsync();
        }

        public async Task<bool> TranslationKeyExistsAsync(int keyId)
        {
            return await _context.TranslationKeys
                .AnyAsync(k => k.Id == keyId && k.IsActive);
        }

        public async Task<bool> LanguageExistsAsync(string languageCode)
        {
            return await _context.Languages
                .AnyAsync(l => l.Code == languageCode);
        }

        public async Task<TranslationValue?> GetTranslationValueAsync(int keyId, string languageCode)
        {
            return await _context.TranslationValues
                .FirstOrDefaultAsync(t => t.TranslationKeyId == keyId && t.LanguageCode == languageCode);
        }

        public async Task SaveTranslationAsync(TranslationValue translation)
        {
            _context.TranslationValues.Add(translation);
            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

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

        public async Task UpsertBulkAsync(
            List<dynamic> items,
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
                    found.Value = item.Value;
                    found.UpdatedByEmpId = empId;
                    found.UpdatedAt = DateTime.UtcNow;
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

        public async Task<TranslationKey?> GetKeyByIdAsync(int id)
        {
            return await _context.TranslationKeys
                .Include(k => k.Translations)
                .FirstOrDefaultAsync(k => k.Id == id && k.IsActive);
        }

        public async Task DeleteValuesAsync(List<TranslationValue> values)
        {
            if (values.Count == 0)
            {
                return;
            }

            _context.TranslationValues.RemoveRange(values);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteKeyAsync(TranslationKey key)
        {
            _context.TranslationKeys.Remove(key);
            await _context.SaveChangesAsync();
        }
    }
}
