using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Translation.Models.Entities
{
    // Translation value for a key and language.
    public class TranslationValue
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // FK to TranslationKey (canonical)
        public int TranslationKeyId { get; set; }

        // Backwards-compatible alias used across the codebase
        // ⚠️ This property is NOT mapped to database - it's just a convenience wrapper
        [NotMapped]
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
