namespace TranslationService.DTO
{
    public class AddTranslationRequest
    {
        public int KeyId { get; set; }

        public string LanguageCode { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;
    }
}
