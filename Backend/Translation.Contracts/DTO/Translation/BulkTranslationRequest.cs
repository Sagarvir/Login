namespace Translation.Contracts.DTO.Translation
{
    // Request payload for bulk translation insert.
    public class BulkTranslationRequest
    {
        public List<AddTranslationRequest> Translations { get; set; } = new();
    }

    // Item describing a translation in bulk operations.
    public class BulkTranslationItem
    {
        public int KeyId { get; set; }
        public string Key { get; set; } = string.Empty;
        public string LanguageCode { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    // Request payload for bulk translation upsert.
    public class BulkTranslationRequestV2
    {
        public List<BulkTranslationItem> Translations { get; set; } = new();
    }
}
