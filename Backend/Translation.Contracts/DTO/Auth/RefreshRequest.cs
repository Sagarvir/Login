using System.ComponentModel.DataAnnotations;

namespace Translation.Contracts.DTO.Auth
{
    // Request payload for refreshing access tokens.
    public class RefreshRequest
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
