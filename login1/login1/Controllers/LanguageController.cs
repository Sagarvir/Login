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

