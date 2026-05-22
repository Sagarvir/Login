using global::login1.Data;
using global::login1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TranslationService.DTO.Translation;
using TranslationService.Services.Interfaces;
namespace login1.Controllers
    {
        [ApiController]
        [Route("api/[controller]")]
        public class TranslationValueController : ControllerBase
        {
            private readonly AppDbContext _context;
            private readonly ITranslationService _translationService;

            public TranslationValueController(AppDbContext context, ITranslationService translationService)
            {
                _context = context;
                _translationService = translationService;
            }

            // ✅ UPSERT (Create or Update Translation)
            [HttpPost]
            [Authorize(Roles = "Translator,Admin")]
            public async Task<IActionResult> UpsertTranslation(AddTranslationRequest request)
            {
                try
                {
                    // 🔴 Validate input
                    if (request.KeyId <= 0)
                        return BadRequest("Valid KeyId is required.");

                    if (string.IsNullOrWhiteSpace(request.LanguageCode))
                        return BadRequest("LanguageCode is required.");

                    // 🔴 Normalize language to lowercase for database matching
                    var language = request.LanguageCode.Trim().ToUpper();

                    // 🔴 Check key exists
                    var keyExists = await _context.TranslationKeys
                        .AnyAsync(k => k.Id == request.KeyId && k.IsActive);

                    if (!keyExists)
                        return NotFound("Translation key not found.");

                    // 🔴 Check language exists
                    var languageExists = await _context.Languages
                        .AnyAsync(l => l.Code == language);

                    if (!languageExists)
                        return BadRequest($"Language '{request.LanguageCode}' is not supported. Supported languages: en, es, fr, de, ja");

                    // 🔴 Get empId
                    var empId = User.FindFirst("empId")?.Value;
                    if (string.IsNullOrEmpty(empId))
                        return Unauthorized("Invalid token.");

                    // 🔍 Check if translation already exists
                    var existing = await _context.TranslationValues
                        .FirstOrDefaultAsync(t => t.TranslationKeyId == request.KeyId && t.LanguageCode == language);

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
                            TranslationKeyId = request.KeyId,
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
                catch (Exception ex)
                {
                    return StatusCode(500, new { error = "Failed to save translation", details = ex.InnerException?.Message ?? ex.Message });
                }
            }

            // ✅ GET Translation by Key + Language (for dropdown UI)
            [HttpGet]
            [Authorize]
            public async Task<IActionResult> GetTranslation(int keyId, string languageCode)
            {
                var language = languageCode.Trim().ToUpper();

                var translation = await _context.TranslationValues
                    .Where(t => t.TranslationKeyId == keyId && t.LanguageCode == language)
                    .Select(t => new
                    {
                        t.TranslationKeyId,
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
                    .Where(t => t.TranslationKeyId == keyId)
                    .Select(t => new
                    {
                        t.LanguageCode,
                        t.Value,

                    })
                    .ToListAsync();

                return Ok(translations);
            }
        [HttpPost("bulk")]
        [Authorize(Roles = "Translator,Admin")]
        public async Task<IActionResult> UpsertTranslations(BulkTranslationRequest request)
        {
            var empId = User.FindFirst("empId")?.Value;
            if (string.IsNullOrEmpty(empId))
                return Unauthorized("Invalid token.");

            try
            {
                var result = await _translationService.UpsertTranslationsAsync(request, empId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("bulk-v2")]
        [Authorize(Roles = "Translator,Admin")]
        public async Task<IActionResult> UpsertTranslationsV2(BulkTranslationRequestV2 request)
        {
            var empId = User.FindFirst("empId")?.Value;
            if (string.IsNullOrEmpty(empId))
                return Unauthorized("Invalid token.");

            try
            {
                var result = await _translationService.UpsertTranslationsV2Async(request, empId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("with-translations")]
        [Authorize(Roles ="Translator,Creator,Admin,Viewer")]
        public async Task<IActionResult> GetKeysWithTranslations(string languageCode)
        {
            if (string.IsNullOrWhiteSpace(languageCode))
                return BadRequest("LanguageCode is required.");

            var language = languageCode.Trim().ToUpper();

            var result = await _context.TranslationKeys
                .Where(k => k.IsActive)
                .Select(k => new TranslationService.DTO.Translation.TranslationKeyWithValueDto
                {
                    KeyId = k.Id,
                    Key = k.KeyName,
                    OriginalText = k.OriginalText,
                    ProjectId = k.ProjectId,
                    Value = k.Translations
                        .Where(t => t.LanguageCode == language)
                        .Select(t => t.Value)
                        .FirstOrDefault() ?? ""
                })
                .ToListAsync();

            return Ok(result);
        }
    }
    }

