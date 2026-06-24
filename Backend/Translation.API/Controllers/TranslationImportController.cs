using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Translation.Service.Interfaces;

namespace Translation.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    
    public class TranslationImportController : ControllerBase
    {
        private readonly ITranslationImportService _importService;

        public TranslationImportController(ITranslationImportService importService)
        {
            _importService = importService;
        }

        [HttpPost("keys")]
        [Authorize(Roles = "Creator")]
        public async Task<IActionResult> ImportKeys(IFormFile file, [FromForm] int projectId)
        {
            var empId = User.FindFirst("EmployeeId")?.Value
                        ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(empId))
                return Unauthorized("Invalid token.");

            var result = await _importService.ImportKeysAsync(file, empId, projectId);

            if (result.Errors.Any())
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("translations")]
        [Authorize(Roles = "Translator,translator")]

        public async Task<IActionResult> ImportTranslations(IFormFile file)
        {
            var empId = User.FindFirst("empId")?.Value
                        ?? User.FindFirst("EmployeeId")?.Value
                        ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var preferredLanguageCode = User.FindFirst("preferred_language")?.Value;

            if (string.IsNullOrWhiteSpace(empId))
                return Unauthorized("Invalid token.");

            if (string.IsNullOrWhiteSpace(preferredLanguageCode))
                return BadRequest(new { message = "Preferred language is missing in token." });

            var result = await _importService.ImportTranslationsAsync(
                file,
                empId,
                preferredLanguageCode.ToLower());

            if (result.Errors.Any())
                return BadRequest(result);

            return Ok(result);
        }
    }
}