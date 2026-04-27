using System.ComponentModel.DataAnnotations;

namespace login1.Models
{
    public class RefreshRequest
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
