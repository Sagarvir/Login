namespace login1.Models
{
    using System.Text.Json.Serialization;

    public class TranslationValue
    {
        public int Id { get; set; }

        public int KeyId { get; set; }

        [JsonIgnore]
        public TranslationKey Key { get; set; } = null!;

        public string LanguageCode { get; set; } = string.Empty; // EN, FR, ES

        public string Value { get; set; } = string.Empty;

        public string Status { get; set; } = "Completed";

        public string UpdatedByEmpId { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
