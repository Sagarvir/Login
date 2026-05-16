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

        public Task<List<(string KeyName, int ProjectId)>> GetExistingKeys(List<NormalizedKeyDto> keys)
        {
            throw new NotImplementedException();
        }
    }
}
