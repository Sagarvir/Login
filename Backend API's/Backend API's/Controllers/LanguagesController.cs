using Backend_API_s.Dtos;
using Backend_API_s.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend_API_s.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LanguagesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LanguagesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var languages = await _context.Languages.ToListAsync();
            return Ok(languages);
        }

        [HttpPost]
        public async Task<IActionResult> Create(LanguageCreateDto dto)
        {
            var language = new Language
            {
                Code = dto.Code,
                Name = dto.Name,
                CreatedAt = DateTime.UtcNow
            };

            _context.Languages.Add(language);
            await _context.SaveChangesAsync();
            return Ok(language);
        }
    }
}
