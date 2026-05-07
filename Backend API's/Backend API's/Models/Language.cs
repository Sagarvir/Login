namespace Backend_API_s.Models
{
    public class Language
    {
        public string Code { get; set; }  // EN, ES, JP
        public string Name { get; set; }

        public DateTime CreatedAt { get; set; }

        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<Translation> Translations { get; set; } = new List<Translation>();
    }
}
