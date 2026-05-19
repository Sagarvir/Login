using login1.Models;
using TranslationService.DTO.Translation;

namespace TranslationService.Repositories.Interfaces
{
    public interface ITranslationRepository
    {
        Task<bool> KeyExists(string keyName, int projectId);
        Task AddKey(TranslationKey key);
        Task AddKeys(List<TranslationKey> keys);
        Task<List<TranslationKey>> GetAllKeys();
        Task<List<(string KeyName, int ProjectId)>> GetExistingKeys(List<NormalizedKeyDto> keys);
        Task<List<int>> GetValidKeyIdsAsync(List<int> keyIds);
        Task<List<TranslationValue>> GetExistingTranslationsAsync(List<int> keyIds);
        Task UpsertBulkAsync(
            List<dynamic> items,
            List<TranslationValue> existing,
            string empId);
        Task<TranslationKey?> GetKeyByIdAsync(int id);
        Task DeleteValuesAsync(List<TranslationValue> values);
        Task DeleteKeyAsync(TranslationKey key);
    }
}
