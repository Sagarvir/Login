namespace Translation.Contracts.DTO.Translation
{
    // Response payload for a translation lookup.
    public class TranslationResponse
    {
        public string KeyName { get; set; } = string.Empty;

        public string LanguageCode { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;

        

        public List<string> Projects { get; set; } = new List<string>();
    }
}
