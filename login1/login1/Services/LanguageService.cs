using login1.Models;
using TranslationService.Repositories.Interfaces;
using TranslationService.Services.Interfaces;
using TranslationService.DTO.Languages;
namespace TranslationService.Services
{
    public class LanguageService : ILanguageService
    {
        private readonly ILanguageRepository _repo;

        public LanguageService(ILanguageRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<Language>> GetLanguagesAsync()
        {
            return await _repo.GetLanguagesAsync();
        }

        public async Task<Language> AddLanguageAsync(AddLanguage language)
        {
            var lang = new Language
            {
                Code = language.code,
                Name = language.name
            };

            lang.Id = 0;

            if (string.IsNullOrWhiteSpace(language.code) && !string.IsNullOrWhiteSpace(language.name))
            {
                language.code = language.name.Substring(0, Math.Min(2, language.name.Length)).ToLowerInvariant();
            }

            await _repo.AddLanguageAsync(lang);
            return lang;
        }

        public async Task DeleteLanguageAsync(int id)
        {
            var lang = await _repo.GetLanguageByIdAsync(id);
            if (lang == null)
                throw new Exception("Language not found.");

            await _repo.DeleteLanguageAsync(lang);
        }
    }
}
