using TranslationService.DTO.Translation;

namespace TranslationService.Services.Interfaces
{
    public interface ITranslationService
    {
        Task<object> CreateKey(CreateKeyRequest request, string empId);
        Task<object> CreateKeys(CreateKeysRequest request, string empId);
        Task<object> GetAllKeys();
    }
}
