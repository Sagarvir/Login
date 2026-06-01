
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
namespace Translation.Models.Entities
{
    // Canonical translation key stored per project.
    public class TranslationKey
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public required string KeyName { get; set; } = string.Empty;
        public required string OriginalText { get; set; } // ✅ NEW
        public int ProjectId { get; set; }       // ✅ NEW


        public string CreatedByEmpId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        // Navigation
        [JsonIgnore]
        public ICollection<TranslationValue> Translations { get; set; } = new List<TranslationValue>();

        [JsonIgnore]
        public ICollection<KeyProject> KeyProjects { get; set; } = new List<KeyProject>();


    }
}