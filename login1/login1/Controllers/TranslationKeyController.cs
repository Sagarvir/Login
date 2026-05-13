using login1.Data;
using login1.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace login1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TranslationKeyController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TranslationKeyController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ Create Key
        [HttpPost]
        [Authorize(Roles = "Creator,Admin")]
        public async Task<IActionResult> CreateKey(CreateKeyRequest request)
        {
            // 🔴 Normalize KeyName
            var keyName = request.KeyName.Trim().ToUpper();

            // 🔴 Validate input
            if (string.IsNullOrWhiteSpace(request.OriginalText))
                return BadRequest("Original text is required.");

            if (request.ProjectId <= 0)
                return BadRequest("Valid ProjectId is required.");

            // 🔴 Check duplicate (per project - better design)
            var exists = await _context.TranslationKeys
                .AnyAsync(k => k.KeyName == keyName
                            && k.ProjectId == request.ProjectId
                            && k.IsActive);

            if (exists)
                return BadRequest("Key already exists in this project.");

            // 🔴 Get empId from token
            var empId = User.FindFirst("empId")?.Value;

            if (string.IsNullOrEmpty(empId))
                return Unauthorized("Invalid token.");

            // 🟢 Create Key WITH OriginalText
            var key = new TranslationKey
            {
                KeyName = keyName,
                OriginalText = request.OriginalText,   // ✅ NEW
                ProjectId = request.ProjectId,         // ✅ NEW
                CreatedByEmpId = empId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.TranslationKeys.Add(key);

            await _context.SaveChangesAsync();

            // ❌ REMOVED: No more EN in TranslationValues

            return Ok(new
            {
                message = "Key created successfully.",
                keyId = key.Id
            });
        }

        // ✅ Create Multiple Keys
        [HttpPost("bulk")]
        [Authorize(Roles = "Creator,Admin")]
        public async Task<IActionResult> CreateKeys(CreateKeysRequest request)
        {
            if (request.Keys == null || request.Keys.Count == 0)
                return BadRequest("At least one key is required.");

            var empId = User.FindFirst("empId")?.Value;

            if (string.IsNullOrEmpty(empId))
                return Unauthorized("Invalid token.");

            var normalizedKeys = request.Keys
                .Select(k => new
                {
                    KeyName = k.KeyName?.Trim().ToUpper(),
                    OriginalText = k.OriginalText?.Trim(),
                    ProjectId = k.ProjectId
                })
                .ToList();

            if (normalizedKeys.Any(k => string.IsNullOrWhiteSpace(k.KeyName)))
                return BadRequest("KeyName is required for all keys.");

            if (normalizedKeys.Any(k => string.IsNullOrWhiteSpace(k.OriginalText)))
                return BadRequest("Original text is required for all keys.");

            if (normalizedKeys.Any(k => k.ProjectId <= 0))
                return BadRequest("Valid ProjectId is required for all keys.");

            var projectIds = normalizedKeys.Select(k => k.ProjectId).Distinct().ToList();
            var keyNames = normalizedKeys.Select(k => k.KeyName!).Distinct().ToList();

            var existingCandidates = await _context.TranslationKeys
                .Where(k => k.IsActive
                            && projectIds.Contains(k.ProjectId)
                            && keyNames.Contains(k.KeyName))
                .Select(k => new { k.KeyName, k.ProjectId })
                .ToListAsync();

            var existingKeyNames = existingCandidates
                .Where(e => normalizedKeys.Any(n => n.ProjectId == e.ProjectId && n.KeyName == e.KeyName))
                .ToList();

            if (existingKeyNames.Count > 0)
                return BadRequest(new
                {
                    message = "Some keys already exist in this project.",
                    keys = existingKeyNames
                });

            var keysToAdd = normalizedKeys.Select(k => new TranslationKey
            {
                KeyName = k.KeyName!,
                OriginalText = k.OriginalText!,
                ProjectId = k.ProjectId,
                CreatedByEmpId = empId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            }).ToList();

            _context.TranslationKeys.AddRange(keysToAdd);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Keys created successfully.",
                keyIds = keysToAdd.Select(k => k.Id).ToList()
            });
        }

    }
}
