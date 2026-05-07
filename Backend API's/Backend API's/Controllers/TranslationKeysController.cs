using Backend_API_s.Dtos;
using Backend_API_s.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend_API_s.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TranslationKeysController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TranslationKeysController(AppDbContext context)
        {
            _context = context;
        }

        // Get all keys
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var keys = await _context.TranslationKeys
                .Include(k => k.Translations)
                .ToListAsync();

            return Ok(keys);
        }

        // Create new key (English text)
        [HttpPost]
        public async Task<IActionResult> Create(TranslationKeyCreateDto dto)
        {
            var key = new TranslationKey
            {
                Key = dto.Key,
                DefaultText = dto.DefaultText,
                CreatedAt = DateTime.UtcNow
            };

            _context.TranslationKeys.Add(key);
            await _context.SaveChangesAsync();

            return Ok(key);
        }
    }
}
