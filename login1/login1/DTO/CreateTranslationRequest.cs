namespace TranslationService.DTO
{
    public class CreateTranslationRequest
    {
        public int KeyId { get; set; }
        public string LanguageCode { get; set; }
        public string Value { get; set; }
    }
}
