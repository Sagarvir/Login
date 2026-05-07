namespace Backend_API_s.Dtos
{
    public sealed class TranslationCreateDto
    {
        public int TranslationKeyId { get; set; }
        public string LanguageCode { get; set; } = string.Empty;
        public string TranslatedText { get; set; } = string.Empty;
    }
}
