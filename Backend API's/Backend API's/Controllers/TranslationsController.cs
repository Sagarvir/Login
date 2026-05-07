using Backend_API_s.Dtos;
using Backend_API_s.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend_API_s.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TranslationsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TranslationsController(AppDbContext context)
        {
            _context = context;
        }

        // Add translation
        [HttpPost]
        public async Task<IActionResult> Create(TranslationCreateDto dto)
        {
            var translation = new Translation
            {
                TranslationKeyId = dto.TranslationKeyId,
                LanguageCode = dto.LanguageCode,
                TranslatedText = dto.TranslatedText,
                CreatedAt = DateTime.Now
            };

            _context.Translations.Add(translation);
            await _context.SaveChangesAsync();

            return Ok(translation);
        }

        //  Get translations by language (IMPORTANT API)
        [HttpGet("by-language/{langCode}")]
        public async Task<IActionResult> GetByLanguage(string langCode)
        {
            var data = await _context.Translations
                .Where(t => t.LanguageCode == langCode)
                .Include(t => t.TranslationKey)
                .Select(t => new
                {
                    t.TranslationKey.Key,
                    Text = t.TranslatedText
                })
                .ToListAsync();

            return Ok(data);
        }
    }
}
