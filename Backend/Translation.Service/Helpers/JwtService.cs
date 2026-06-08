using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Translation.Models.Entities;
namespace Translation.Service.Helpers
{
    // Helper for issuing and validating JWTs and refresh tokens.
    public class JwtService
    {
        private readonly byte[] _jwtKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _accessTokenExpiryMinutes;
        private readonly int _refreshTokenExpiryDays;

        // Constructor reads JWT settings from configuration and validates them.
        public JwtService(IConfiguration config)
        {
            _jwtKey = Encoding.UTF8.GetBytes(
                config["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is missing.")
            );
            _issuer = config["Jwt:Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer is missing.");
            _audience = config["Jwt:Audience"] ?? throw new InvalidOperationException("Jwt:Audience is missing.");

            if (!int.TryParse(config["Jwt:ExpiryMinutes"], out _accessTokenExpiryMinutes) || _accessTokenExpiryMinutes <= 0)
                throw new InvalidOperationException("Jwt:ExpiryMinutes must be a positive integer.");

            if (!int.TryParse(config["Jwt:RefreshTokenExpiryDays"], out _refreshTokenExpiryDays) || _refreshTokenExpiryDays <= 0)
                throw new InvalidOperationException("Jwt:RefreshTokenExpiryDays must be a positive integer.");
        }

        // Generates a JWT for the given user, including claims for ID, role, and preferred language.
        public string GenerateToken(User user)
        {
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.EmployeeId.ToLower()),
            new Claim(ClaimTypes.Role, user.Role?.Name ?? "Viewer"),
            new Claim("empId", user.EmployeeId.ToLower()),
            new Claim("preferred_language", user.PreferredLanguage?.Code?.ToLower() ?? "en")

        }; //is an array of type claims , and these are the elements inside it ;

            var key = new SymmetricSecurityKey(_jwtKey);

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                // notBefore: DateTime.UtcNow,
                expires: GetAccessTokenExpiryUtc(),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Calculates the expiration time for access tokens based on the configured expiry minutes.
        public DateTime GetAccessTokenExpiryUtc()
        {
            return DateTime.UtcNow.AddMinutes(_accessTokenExpiryMinutes);
        }

        // Calculates the expiration time for refresh tokens based on the configured expiry days.
        public DateTime GetRefreshTokenExpiryUtc()
        {
            return DateTime.UtcNow.AddDays(_refreshTokenExpiryDays);
        }

        // Generates a secure random refresh token and returns it as a base64 string.
        public string GenerateRefreshToken()
        {
            var random = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(random);
        }

        // Hashes a refresh token using SHA256 and returns the hash as a lowercase hexadecimal string.
        public string HashToken(string token)
        {
            var bytes = Encoding.UTF8.GetBytes(token);
            var hash = SHA256.HashData(bytes);
            return Convert.ToHexString(hash).ToLower(CultureInfo.InvariantCulture);
        }
    }
}