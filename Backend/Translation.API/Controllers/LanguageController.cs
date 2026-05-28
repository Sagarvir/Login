using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Translation.Contracts.DTO.Languages;
using Translation.Service.Interfaces;

namespace Translation.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LanguageController : ControllerBase
    {
        private readonly ILanguageService _languageService;

        public LanguageController(ILanguageService languageService)
        {
            _languageService = languageService;
        }

        // GET all languages
        [HttpGet]
        [Authorize(Roles = "Admin,Creator,Translator")]
        public async Task<IActionResult> GetLanguages()
        {
            var languages = await _languageService.GetLanguagesAsync();
            return Ok(languages);
        }

        // ADD new language
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddLanguage(AddLanguage language)
        {
            var result = await _languageService.AddLanguageAsync(language);
            return Ok(result);
        }

        // DELETE language
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteLanguage(int id)
        {
            try
            {
                await _languageService.DeleteLanguageAsync(id);
                return Ok("Deleted");
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}

