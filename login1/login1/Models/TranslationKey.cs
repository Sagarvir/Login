using login1.Models;
using System.Text.Json.Serialization;

public class TranslationKey
{
    public int Id { get; set; }

    public string KeyName { get; set; } = string.Empty;
    public string OriginalText { get; set; } // ✅ NEW
    public int ProjectId { get; set; }       // ✅ NEW


    public string CreatedByEmpId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    // Navigation
    [JsonIgnore]
    public ICollection<TranslationValue> Translations { get; set; } = new List<TranslationValue>();

    [JsonIgnore]
    public ICollection<KeyProject> KeyProjects { get; set; } = new List<KeyProject>();

    // Additional Navigation properties
    public List<TranslationValue> TranslationValues { get; set; } = new();
}