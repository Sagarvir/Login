using System.ComponentModel.DataAnnotations;

namespace login1.Models
{
    public class LoginRequest
    {
        [Required]
        [MinLength(3)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        public int Role_id { get; set; } // 1 for admin, 2 for user, 3 for viewer
    }
}
