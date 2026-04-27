namespace login1.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty; // later we hash this

        public int Role_id { get; set; } // 1 for admin, 2 for user
    }
}
