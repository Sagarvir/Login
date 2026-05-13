    using global::login1.Data;
    using global::login1.Models;
    using login1.Data;
    using login1.Models;
    using login1.Models.DTO;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

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

            //  BULK UPSERT (Create or Update Translations)
            [HttpPost("bulk")]
            [Authorize(Roles = "Translator,Admin")]
            public async Task<IActionResult> UpsertTranslations(BulkTranslationRequest request)
            {
                if (request.Translations == null || request.Translations.Count == 0)
                    return BadRequest("At least one translation is required.");

                var empId = User.FindFirst("empId")?.Value;
                if (string.IsNullOrEmpty(empId))
                    return Unauthorized("Invalid token.");

                var normalizedItems = request.Translations
                    .Select(t => new
                    {
                        t.KeyId,
                        LanguageCode = t.LanguageCode.Trim().ToUpper(),
                        t.Value
                    })
                    .ToList();

                if (normalizedItems.Any(t => t.KeyId <= 0))
                    return BadRequest("Valid KeyId is required for all translations.");

                if (normalizedItems.Any(t => string.IsNullOrWhiteSpace(t.LanguageCode)))
                    return BadRequest("LanguageCode is required for all translations.");

                var keyIds = normalizedItems.Select(t => t.KeyId).Distinct().ToList();

                var keyExistsMap = await _context.TranslationKeys
                    .Where(k => k.IsActive && keyIds.Contains(k.Id))
                    .Select(k => k.Id)
                    .ToListAsync();

                if (keyExistsMap.Count != keyIds.Count)
                    return NotFound("One or more translation keys not found.");

                var existingTranslations = await _context.TranslationValues
                    .Where(t => keyIds.Contains(t.KeyId))
                    .ToListAsync();

                foreach (var item in normalizedItems)
                {
                    var existing = existingTranslations
                        .FirstOrDefault(t => t.KeyId == item.KeyId && t.LanguageCode == item.LanguageCode);

                    if (existing != null)
                    {
                        existing.Value = item.Value;
                        existing.UpdatedByEmpId = empId;
                        existing.UpdatedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        _context.TranslationValues.Add(new TranslationValue
                        {
                            KeyId = item.KeyId,
                            LanguageCode = item.LanguageCode,
                            Value = item.Value,
                            UpdatedByEmpId = empId,
                            UpdatedAt = DateTime.UtcNow
                        });
                    }
                }

                await _context.SaveChangesAsync();

                return Ok("Translations saved successfully.");
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

