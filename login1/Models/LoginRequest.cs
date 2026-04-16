namespace login1.Models
{
    public class LoginRequest
    {
        public string? Username { get; set; }
        public string? Password { get; set; }

        public int Role_id { get; set; } // 1 for admin, 2 for user, 3 for viewer
    }
}
