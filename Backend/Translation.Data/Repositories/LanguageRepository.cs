using Microsoft.EntityFrameworkCore;
using Translation.DAO.Data;
using Translation.DAO.Repositories.Interfaces;
using Translation.Models.Entities;

namespace Translation.DAO.Repositories
{
    // Data access implementation for languages.
    public class LanguageRepository : ILanguageRepository
    {
        private readonly AppDbContext _context;

        public LanguageRepository(AppDbContext context)
        {
            _context = context;
        }

        // Retrieves all languages from the database.
        public async Task<List<Language>> GetLanguagesAsync()
        {
            return await _context.Languages.ToListAsync();
        }

        // Adds a new language to the database.
        public async Task AddLanguageAsync(Language language)
        {
            _context.Languages.Add(language);
            await _context.SaveChangesAsync();
        }
        // Retrieves a language by its ID.

        public async Task<Language?> GetLanguageByIdAsync(int id)
        {
            return await _context.Languages.FindAsync(id);
        }


        // Delete an existing language in the database.
        public async Task DeleteLanguageAsync(Language language)
        {
            _context.Languages.Remove(language);
            await _context.SaveChangesAsync();
        }

        // Saves changes to the database.
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
