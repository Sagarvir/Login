using Translation.Models.Entities;
using Translation.Contracts.DTO.Languages;
namespace Translation.Service.Interfaces
{
    // Service contract for language management.
    public interface ILanguageService
    {
        Task<List<Language>> GetLanguagesAsync();
        Task<Language> AddLanguageAsync(AddLanguage language);
        Task DeleteLanguageAsync(int id);
    }
}
