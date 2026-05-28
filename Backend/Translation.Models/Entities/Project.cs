using System.Text.Json.Serialization;
namespace Translation.Models.Entities
{
    

    public class Project
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        // Navigation
        [JsonIgnore]
        public ICollection<KeyProject> KeyProjects { get; set; } = new List<KeyProject>();
    }
}
