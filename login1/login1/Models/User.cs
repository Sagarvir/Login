namespace login1.Models
{
    public class User
    {
        public int Id { get; set; }

        public string EmployeeId { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int? PreferredLanguageId { get; set; }
        public Language? PreferredLanguage { get; set; }

        public int? RoleId { get; set; }   // ✅ FIXED
        public Role? Role { get; set; }    // ✅ navigation
    }
}
