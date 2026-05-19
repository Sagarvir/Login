namespace TranslationService.DTO.Translation
{
    public class BulkTranslationRequest
    {
        public List<AddTranslationRequest> Translations { get; set; } = new();
    }
}
