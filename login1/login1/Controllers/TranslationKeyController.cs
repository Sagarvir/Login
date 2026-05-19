using login1.Data;
using login1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TranslationService.DTO.Translation;
using TranslationService.Services.Interfaces;



namespace login1.Controllers
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

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllKeys()
        {
            var result = await _service.GetAllKeys();
            return Ok(result);
        }
        [HttpDelete("{id}")]
        [Authorize(Roles = "Creator,Admin")]
        public async Task<IActionResult> DeleteKey(int id)
        {
            try
            {
                var empId = User.FindFirst("empId")?.Value;
                await _service.DeleteKey(id, empId);
                return Ok("Key deleted successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}