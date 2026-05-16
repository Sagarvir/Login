using global::login1.Data;
using global::login1.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TranslationService.DTO.Translation;

namespace login1.Controllers
    {
        [ApiController]
        [Route("api/[controller]")]
        public class TranslationValueController : ControllerBase
        {
            private readonly AppDbContext _context;

            public TranslationValueController(AppDbContext context)
            {
                _context = context;
            }

            // ✅ UPSERT (Create or Update Translation)
            [HttpPost]
            [Authorize(Roles = "Translator,Admin")]
            public async Task<IActionResult> UpsertTranslation(AddTranslationRequest request)
            {
                // 🔴 Validate input
                if (request.KeyId <= 0)
                    return BadRequest("Valid KeyId is required.");

                if (string.IsNullOrWhiteSpace(request.LanguageCode))
                    return BadRequest("LanguageCode is required.");

                // 🔴 Normalize language
                var language = request.LanguageCode.Trim().ToUpper();

                // 🔴 Check key exists
                var keyExists = await _context.TranslationKeys
                    .AnyAsync(k => k.Id == request.KeyId && k.IsActive);

                if (!keyExists)
                    return NotFound("Translation key not found.");

                // 🔴 Get empId
                var empId = User.FindFirst("empId")?.Value;
                if (string.IsNullOrEmpty(empId))
                    return Unauthorized("Invalid token.");

                // 🔍 Check if translation already exists
                var existing = await _context.TranslationValues
                    .FirstOrDefaultAsync(t => t.KeyId == request.KeyId && t.LanguageCode == language);

                if (existing != null)
                {
                    // 🟡 UPDATE
                    existing.Value = request.Value;
                    existing.UpdatedByEmpId = empId;
                    existing.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    // 🟢 CREATE
                    var translation = new TranslationValue
                    {
                        KeyId = request.KeyId,
                        LanguageCode = language,
                        Value = request.Value,
                        UpdatedByEmpId = empId,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.TranslationValues.Add(translation);
                }

                await _context.SaveChangesAsync();

                return Ok("Translation saved successfully.");
            }

            // ✅ GET Translation by Key + Language (for dropdown UI)
            [HttpGet]
            [Authorize]
            public async Task<IActionResult> GetTranslation(int keyId, string languageCode)
            {
                var language = languageCode.Trim().ToUpper();

                var translation = await _context.TranslationValues
                    .Where(t => t.KeyId == keyId && t.LanguageCode == language)
                    .Select(t => new
                    {
                        t.KeyId,
                        t.LanguageCode,
                        t.Value,
                        
                    })
                    .FirstOrDefaultAsync();

                if (translation == null)
                    return Ok(new { value = "" }); // empty for UI

                return Ok(translation);
            }

            // ✅ GET All Translations for a Key (optional, useful later)
            [HttpGet("all/{keyId}")]
            [Authorize]
            public async Task<IActionResult> GetAllTranslations(int keyId)
            {
                var translations = await _context.TranslationValues
                    .Where(t => t.KeyId == keyId)
                    .Select(t => new
                    {
                        t.LanguageCode,
                        t.Value,
                       
                    })
                    .ToListAsync();

                return Ok(translations);
            }
        }
    }

