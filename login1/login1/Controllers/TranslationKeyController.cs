using login1.Data;
using login1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TranslationService.DTO;

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

        // ✅ Get All Translation Keys
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllKeys()
        {
            var keys = await _context.TranslationKeys
                .Where(k => k.IsActive)
                .OrderByDescending(k => k.CreatedAt)
                .Select(k => new
                {
                    k.KeyName,
                    k.OriginalText,
                    k.ProjectId
                })
                .ToListAsync();

            return Ok(keys);
        }

    }
}
