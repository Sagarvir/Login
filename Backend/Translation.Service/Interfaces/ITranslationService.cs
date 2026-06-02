using Translation.Contracts.DTO.Translation;
using Translation.Models.Entities;

namespace Translation.Service.Interfaces
{
    // Service contract for translation workflows.
    public interface ITranslationService
    {
        Task<object> CreateKey(CreateKeyRequest request, string empId);
        Task<object> CreateKeys(CreateKeysRequest request, string empId);
        Task<object> GetAllKeys();
        Task<string> InsertTranslationsAsync(BulkTranslationRequest request, string empId);

        Task<string> InsertTranslationAsync(AddTranslationRequest request, string empId);

        Task<object> GetTranslationAsync(string key_name, string languageCode);
        Task<object> GetAllTranslationsAsync(string key_name);
        Task<List<TranslationKeyWithValueDto>> GetKeysWithTranslationsAsync(string languageCode);

        Task DeleteKey(int id);

        Task<string> UpsertTranslationsV2Async(BulkTranslationRequestV2 request, string empId);
        Task<PublishTranslationResponse> PublishTranslationsAsync();
        Task<PublishTranslationResponse> PublishLanguageAsync(string languageCode);
    }
}
