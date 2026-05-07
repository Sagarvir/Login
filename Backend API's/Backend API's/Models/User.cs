namespace Backend_API_s.Models
{
    public enum UserRole
    {
        Admin,
        Translator,
        Creator
    }

    public class User
    {
        public int UserId { get; set; }
        public string PasswordHash { get; set; }
        public UserRole Role { get; set; }

        public string PreferredLanguageCode { get; set; }
        public Language PreferredLanguage { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
