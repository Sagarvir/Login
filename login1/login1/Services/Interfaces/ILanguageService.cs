using login1.Models;
using TranslationService.DTO.Languages;
namespace TranslationService.Services.Interfaces
{
    public interface ILanguageService
    {
        Task<List<Language>> GetLanguagesAsync();
        Task<Language> AddLanguageAsync(AddLanguage language);
        Task DeleteLanguageAsync(int id);
    }
}
