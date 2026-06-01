using Translation.Contracts.DTO.Translation;
using Translation.Models.Entities;

namespace Translation.DAO.Repositories.Interfaces
{
    // Repository contract for translation keys and values.
    public interface ITranslationRepository
    {
        Task<bool> KeyExists(string keyName, int projectId);
        Task AddKey(TranslationKey key);
        Task AddKeys(List<TranslationKey> keys);
        Task<List<TranslationKey>> GetAllKeys();
        Task<List<(string KeyName, int ProjectId)>> GetExistingKeys(List<CreateKeyItem> keys);
        Task<List<int>> GetValidKeyIdsAsync(List<int> keyIds);
        Task<List<TranslationValue>> GetExistingTranslationsAsync(List<int> keyIds);
        Task InsertBulkAsync(
            List<BulkTranslationItem> items,
            List<TranslationValue> existing,
            string empId);
        Task<bool> TranslationKeyExistsAsync(int keyId);
        Task<bool> LanguageExistsAsync(string languageCode);
        Task<TranslationValue?> GetTranslationValueAsync(int keyId, string languageCode);
        Task SaveTranslationAsync(TranslationValue translation);
        Task SaveChangesAsync();
        Task<TranslationValue?> GetTranslationForUiAsync(int keyId, string languageCode);
        Task<List<TranslationValue>> GetTranslationsByKeyAsync(int keyId);
        Task<List<TranslationKeyWithValueDto>> GetKeysWithTranslationsAsync(string languageCode);
        Task<TranslationKey?> GetKeyByIdAsync(int id);
        Task DeleteValuesAsync(List<TranslationValue> values);
        Task DeleteKeyAsync(TranslationKey key);
        Task<List<TranslationValue>> GetAllTranslationsForPublishAsync();
        Task<List<TranslationValue>> GetTranslationsByLanguageAsync(string languageCode);
        Task SavePublishRecordAsync(TranslationPublish publishRecord);
    }
}
