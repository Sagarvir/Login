
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Translation.Contracts.DTO.Translation;
using Translation.Service.Interfaces;
using TranslationService.DTO.Translation;


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

        // Create (Create or Translation)
        [HttpPost]
        [Authorize(Roles = "Translator")]
        public async Task<IActionResult> InsertTranslation(AddTranslationRequest request)
        {
            try
            {
                var empId = User.FindFirst("empId")?.Value;

                var result = await _translationService.InsertTranslationAsync(request, empId);
                return Ok(new
                {
                    success = true,
                    message = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //GET Translation by Key + Language (for dropdown UI)
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetTranslation(string key_name, string languageCode)
        {
            try
            {
                var result = await _translationService.GetTranslationAsync(key_name, languageCode);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET All Translations for a Key (optional, useful later)
        [HttpGet("all/{keyName}")]
        [Authorize]
        public async Task<IActionResult> GetAllTranslations(string keyName)
        {
            try
            {
                var result = await _translationService.GetAllTranslationsAsync(keyName);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Bulk Insert Translations
        [HttpPost("bulk")]
        [Authorize(Roles = "Translator")]
        public async Task<IActionResult> InsertTranslations(BulkTranslationRequest request)
        {
            var empId = User.FindFirst("empId")?.Value;


            try
            {
                var result = await _translationService.InsertTranslationsAsync(request, empId);

                return Ok(new
                {
                    success = true,
                    message = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Bulk Upsert Translations (new endpoint for upsert)
        [HttpPost("bulk-v2")]
        [Authorize(Roles = "Translator")]
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

        // GET Keys with Translations for a Language (for management UI)
        [HttpGet("with-translations")]
        [Authorize(Roles = "Translator,Creator,Admin,Viewer")]
        public async Task<IActionResult> GetKeysWithTranslations(string? key_name,string languageCode)
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

        // Delete the Translations for a specific Translation KeyName (Translator only)
        [HttpDelete("{keyName}/{languageCode}")]
        [Authorize(Roles = "Translator")]
        public async Task<IActionResult> DeleteTranslations(string keyName, string languageCode)
        {
            try
            {
                var result = await _translationService.DeleteTranslationsAsync(keyName, languageCode);
                return Ok(new
                {
                    success = true,
                    message = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }



        // Publish Translations (Admin/Creator only)
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

        [HttpPost("publish/{languageCode}")]
        [Authorize(Roles = "Admin,Creator")]
        public async Task<IActionResult> PublishLanguage(string languageCode)
        {
            var result =
                await _translationService
                    .PublishLanguageAsync(
                        languageCode);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost("publish/download")]
        [Authorize(Roles = "Admin,Creator")]
        public async Task<IActionResult> PublishTranslationsDownload(
      PublishDownloadRequest request)
        {
            var result =
                await _translationService
                    .PublishTranslationsAsZipAsync(request.FileType);

            if (result.FileBytes == null)
                return BadRequest(result.Message);

            return File(
                result.FileBytes,
                "application/zip",
                result.FileName
            );
        }

        [HttpPost("publish/{languageCode}/download")]
        [Authorize(Roles = "Admin,Creator")]
        public async Task<IActionResult> PublishLanguageDownload(
    string languageCode,
    PublishDownloadRequest request)
        {
            var result =
                await _translationService
                    .PublishLanguageAsZipAsync(
                        languageCode,
                        request.FileType);

            if (result.FileBytes == null)
                return BadRequest(result.Message);

            return File(
                result.FileBytes,
                "application/zip",
                result.FileName
            );
        }

    }
}

