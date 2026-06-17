using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Translation.Service.Interfaces;

namespace Translation.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Creator")]
    public class TranslationImportController : ControllerBase
    {
        private readonly ITranslationImportService _importService;

        public TranslationImportController(ITranslationImportService importService)
        {
            _importService = importService;
        }

        [HttpPost("keys")]
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
    }
}