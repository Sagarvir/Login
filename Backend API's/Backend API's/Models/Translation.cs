namespace Backend_API_s.Models
{
    public class Translation
    {
        public int Id { get; set; }

        public int TranslationKeyId { get; set; }
        public TranslationKey TranslationKey { get; set; }

        public string LanguageCode { get; set; }
        public Language Language { get; set; }

        public string TranslatedText { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
