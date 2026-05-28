using System;
using System.Collections.Generic;
using System.Text;

namespace Translation.Contracts.DTO.Auth
{
    public class AuthResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime AccessTokenExpiresAtUtc { get; set; }
    }
}
