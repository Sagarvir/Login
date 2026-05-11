namespace login1.Models.DTO
{
    public class TranslationResponse
    {
        public string KeyName { get; set; } = string.Empty;

        public string LanguageCode { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public List<string> Projects { get; set; } = new List<string>();
    }
}
