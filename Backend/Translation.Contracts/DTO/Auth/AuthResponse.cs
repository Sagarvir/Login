using System;
using System.Collections.Generic;
using System.Text;

namespace Translation.Contracts.DTO.Auth
{
    // Response payload returned after successful authentication.
    public class AuthResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime AccessTokenExpiresAtUtc { get; set; }
    }
}
