using Translation.Models.Entities;

namespace Translation.DAO.Repositories.Interfaces
{
    public interface ILanguageRepository
    {
        Task<List<Language>> GetLanguagesAsync();
        Task AddLanguageAsync(Language language);
        Task<Language?> GetLanguageByIdAsync(int id);
        Task DeleteLanguageAsync(Language language);
        Task SaveChangesAsync();
    }
}
