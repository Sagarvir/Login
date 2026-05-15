using System;
using System.Text.Json.Serialization;

namespace login1.Models
{
    public class TranslationValue
    {
        public int Id { get; set; }

        // FK to TranslationKey (canonical)
        public int TranslationKeyId { get; set; }

        // Backwards-compatible alias used across the codebase
        public int KeyId
        {
            get => TranslationKeyId;
            set => TranslationKeyId = value;
        }

        // FK to Language via code
        public string LanguageCode { get; set; } = null!;

        public string Value { get; set; } = null!;

        // Optional audit fields referenced by controllers
        public string? UpdatedByEmpId { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties expected by AppDbContext
        public TranslationKey? TranslationKey { get; set; }
        public Language? Language { get; set; }
    }
}
