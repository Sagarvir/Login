namespace login1.Models.DTO
{
    public class BulkTranslationRequest
    {
        public List<TranslationValueItem> Translations { get; set; } = new();
    }

    public class TranslationValueItem
    {
        public int KeyId { get; set; }

        public string LanguageCode { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;
    }
}
