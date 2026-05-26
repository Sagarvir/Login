using login1.Data;
using login1.Models;
using Microsoft.EntityFrameworkCore;
using TranslationService.Repositories.Interfaces;

namespace TranslationService.Repositories
{
    public class LanguageRepository : ILanguageRepository
    {
        private readonly AppDbContext _context;

        public LanguageRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Language>> GetLanguagesAsync()
        {
            return await _context.Languages.ToListAsync();
        }

        public async Task AddLanguageAsync(Language language)
        {
            _context.Languages.Add(language);
            await _context.SaveChangesAsync();
        }

        public async Task<Language?> GetLanguageByIdAsync(int id)
        {
            return await _context.Languages.FindAsync(id);
        }

        public async Task DeleteLanguageAsync(Language language)
        {
            _context.Languages.Remove(language);
            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
