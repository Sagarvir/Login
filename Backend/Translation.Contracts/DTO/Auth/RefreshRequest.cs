using System.ComponentModel.DataAnnotations;

namespace Translation.Contracts.DTO.Auth
{
    public class RefreshRequest
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
