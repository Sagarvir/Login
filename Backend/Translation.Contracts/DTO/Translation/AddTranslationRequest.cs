namespace Translation.Contracts.DTO.Translation
{
    // Request payload for adding a single translation value.
    public class AddTranslationRequest
    {
        public string KeyName { get; set; } = string.Empty;

        public string LanguageCode { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;
    }
}
