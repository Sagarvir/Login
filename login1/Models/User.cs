namespace login1.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; } // later we hash this

        public int Role_id { get; set; } // 1 for admin, 2 for user
    }
}
