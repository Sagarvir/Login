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
    }
}
