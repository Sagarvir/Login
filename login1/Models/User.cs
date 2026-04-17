namespace login1.Models
{
    public class User
    {
        public int Id { get; set; }
        public string EmployeeId { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty; // later we hash this

        public int? Role_id { get; set; } // 1 for admin, 2 for user, assigned by admin later
    }
}
