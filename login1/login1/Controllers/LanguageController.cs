using login1.Data;
using login1.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace login1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LanguageController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LanguageController(AppDbContext context)
        {
            _context = context;
        }

        // GET all languages
        [HttpGet]
        public async Task<IActionResult> GetLanguages()
        {
            var languages = await _context.Languages.ToListAsync();
            return Ok(languages);
        }

        // ADD new language
        [HttpPost]
        public async Task<IActionResult> AddLanguage(Language language)
        {
            // Ensure we don't try to insert an explicit identity value from the client.
            // If client provided an Id, reset it so SQL Server will generate the identity value.
            language.Id = 0;

            // If Code wasn't provided, derive a simple code from the name (first two letters lowercased).
            if (string.IsNullOrWhiteSpace(language.Code) && !string.IsNullOrWhiteSpace(language.Name))
            {
                language.Code = language.Name.Substring(0, Math.Min(2, language.Name.Length)).ToLowerInvariant();
            }

            _context.Languages.Add(language);
            await _context.SaveChangesAsync();
            return Ok(language);
        }

        // DELETE language
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLanguage(int id)
        {
            var lang = await _context.Languages.FindAsync(id);
            if (lang == null) return NotFound();

            _context.Languages.Remove(lang);
            await _context.SaveChangesAsync();
            return Ok("Deleted");
        }
    }
}

