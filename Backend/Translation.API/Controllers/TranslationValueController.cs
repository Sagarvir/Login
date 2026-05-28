
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Translation.Contracts.DTO.Translation;
using Translation.Service.Interfaces;


namespace Translation.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TranslationValueController : ControllerBase
    {
        
        private readonly ITranslationService _translationService;

        public TranslationValueController(ITranslationService translationService)
        {
            _translationService = translationService;
        }

        // ✅ Create (Create or Translation)
        [HttpPost]
        [Authorize(Roles = "Translator,Admin")]
        public async Task<IActionResult> InsertTranslation(AddTranslationRequest request)
        {
            try
            {
                var empId = User.FindFirst("empId")?.Value;
             
                    var result = await _translationService.InsertTranslationAsync(request, empId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // ✅ GET Translation by Key + Language (for dropdown UI)
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetTranslation(int keyId, string languageCode)
        {
            try
            {
                var result = await _translationService.GetTranslationAsync(keyId, languageCode);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // ✅ GET All Translations for a Key (optional, useful later)
        [HttpGet("all/{keyId}")]
        [Authorize]
        public async Task<IActionResult> GetAllTranslations(int keyId)
        {
            try
            {
                var result = await _translationService.GetAllTranslationsAsync(keyId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("bulk")]
        [Authorize(Roles = "Translator,Admin")]
        public async Task<IActionResult> InsertTranslations(BulkTranslationRequest request)
        {
            var empId = User.FindFirst("empId")?.Value;


            try
            {
                var result = await _translationService.InsertTranslationsAsync(request, empId);
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
        [Authorize(Roles = "Translator,Creator,Admin,Viewer")]
        public async Task<IActionResult> GetKeysWithTranslations(string languageCode)
        {
            try
            {
                var result = await _translationService.GetKeysWithTranslationsAsync(languageCode);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("publish")]
        [Authorize(Roles = "Admin,Creator")]
        public async Task<IActionResult> PublishTranslations()
        {
            var result = await _translationService.PublishTranslationsAsync();

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

    }
}

