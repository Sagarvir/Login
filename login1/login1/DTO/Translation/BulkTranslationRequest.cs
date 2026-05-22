namespace TranslationService.DTO.Translation
{
    public class BulkTranslationRequest
    {
        public List<AddTranslationRequest> Translations { get; set; } = new();
    }

    public class BulkTranslationItem
    {
        public int KeyId { get; set; }
        public string Key { get; set; } = string.Empty;
        public string LanguageCode { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    public class BulkTranslationRequestV2
    {
        public List<BulkTranslationItem> Translations { get; set; } = new();
    }
}
