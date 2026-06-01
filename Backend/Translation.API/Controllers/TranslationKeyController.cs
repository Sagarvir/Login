using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Translation.Contracts.DTO.Translation;
using Translation.Service.Interfaces;



namespace Translation.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TranslationKeyController : ControllerBase
    {
        private readonly ITranslationService _service;

        public TranslationKeyController(ITranslationService service)
        {
            _service = service;
        }

        // Create a new translation key
        [HttpPost]
        [Authorize(Roles = "Creator,Admin")]
        public async Task<IActionResult> CreateKey(CreateKeyRequest request)
        {
            try
            {
                var empId = User.FindFirst("empId")?.Value;
                var result = await _service.CreateKey(request, empId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Create multiple translation keys in bulk 
        [HttpPost("bulk")]
        [Authorize(Roles = "Creator,Admin")]
        public async Task<IActionResult> CreateKeys(CreateKeysRequest request)
        {
            try
            {
                var empId = User.FindFirst("empId")?.Value;
                var result = await _service.CreateKeys(request, empId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Get all translation keys
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllKeys()
        {
            var result = await _service.GetAllKeys();
            return Ok(result);
        }

        // Delete a translation key by ID
        [HttpDelete("{id}")]
        [Authorize(Roles = "Creator,Admin")]
        public async Task<IActionResult> DeleteKey(int id)
        {
            try
            {
                await _service.DeleteKey(id);
                return Ok("Key deleted successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}