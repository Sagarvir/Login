namespace login1.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text.Json.Serialization;

    public class KeyProject
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int KeyId { get; set; }

        [JsonIgnore]
        public TranslationKey Key { get; set; } = null!;

        public int ProjectId { get; set; }

        [JsonIgnore]
        public Project Project { get; set; } = null!;
    }
}
