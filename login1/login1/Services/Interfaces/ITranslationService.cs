using TranslationService.DTO.Translation;

namespace TranslationService.Services.Interfaces
{
    public interface ITranslationService
    {
        Task<object> CreateKey(CreateKeyRequest request, string empId);
        Task<object> CreateKeys(CreateKeysRequest request, string empId);
        Task<object> GetAllKeys();
        Task<string> InsertTranslationsAsync(BulkTranslationRequest request, string empId);

        Task<string> InsertTranslationAsync(AddTranslationRequest request, string empId);

        Task<object> GetTranslationAsync(int keyId, string languageCode);
        Task<object> GetAllTranslationsAsync(int keyId);
        Task<List<TranslationKeyWithValueDto>> GetKeysWithTranslationsAsync(string languageCode);

        Task DeleteKey(int id);

        Task<string> UpsertTranslationsV2Async(BulkTranslationRequestV2 request, string empId);
    }
}
