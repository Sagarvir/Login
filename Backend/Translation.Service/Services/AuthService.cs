using Translation.Contracts.DTO.Auth;
using Translation.DAO.Repositories.Interfaces;
using Translation.Models.Entities;
using Translation.Service.Helpers;
using Translation.Service.Interfaces;
using BCrypt.Net;

namespace Translation.Service.Services
{
    // Implements authentication flows backed by repositories and JWTs.
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _repo;
        private readonly JwtService _jwtService;

        // Constructor injects dependencies for user data access and JWT handling.
        public AuthService(IUserRepository repo, JwtService jwtService)
        {
            _repo = repo;
            _jwtService = jwtService;
        }

        // Registers a new user after validating input and ensuring uniqueness.
        public async Task<string> Register(RegisterRequest request)
        {
            var emp = request.EmployeeId?.Trim().ToLower();
            var pwd = request.Password?.Trim();

            var language = await _repo.GetLanguageById(request.PreferredLanguageId);
            if (language == null)
                throw new Exception("Invalid language");
            if (string.IsNullOrWhiteSpace(emp))
                throw new Exception("Employee ID is required");
            if (string.IsNullOrWhiteSpace(pwd))
                throw new Exception("Password is required");

            if (await _repo.UserExists(emp))
                throw new Exception("Employee already exists");

            var role = await _repo.GetDefaultRole();
            if (role == null)
                throw new Exception("Default role not found");

            var user = new User
            {
                EmployeeId = emp!,
                FirstName = request.FirstName!,
                LastName = request.LastName!,
                Password = BCrypt.Net.BCrypt.HashPassword(pwd!),
                RoleId = role.Id,
                PreferredLanguageId = language.Id
            };

            await _repo.AddUser(user);
            await _repo.SaveChanges();

            return "User registered successfully";
        }

        // Authenticates a user and issues JWT tokens if credentials are valid.
        public async Task<AuthResponse> Login(LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.EmployeeId))
                throw new Exception("Employee ID is required");

            if (string.IsNullOrWhiteSpace(request.Password))
                throw new Exception("Password is required");

            var user = await _repo.GetUserByEmployeeId(request.EmployeeId);

            if (user == null)
                throw new Exception("Invalid employee ID");

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
                throw new Exception("Invalid password");

            var accessToken = _jwtService.GenerateToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();
            var hash = _jwtService.HashToken(refreshToken);

            await _repo.AddRefreshToken(new RefreshToken
            {
                UserId = user.Id,
                TokenHash = hash,
                ExpiresAtUtc = _jwtService.GetRefreshTokenExpiryUtc(),
                CreatedAtUtc = DateTime.UtcNow
            });

            await _repo.SaveChanges();

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiresAtUtc = _jwtService.GetAccessTokenExpiryUtc()
            };
        }

        // Assigns a role to a user based on employee ID and role name.
        public async Task<object> AssignRole(AssignRoleRequest request)
        {
            var user = await _repo.GetUserByEmployeeId(request.EmployeeId);
            if (user == null)
                throw new Exception("User not found");

            var role = await _repo.GetRoleByName(request.RoleName);
            if (role == null)
                throw new Exception("Role not found");

            user.RoleId = role.Id;

            await _repo.SaveChanges();

            return new
            {
                message = "Role assigned successfully",
                user.EmployeeId,
                role.Name
            };
        }

        // Refreshes JWT tokens by validating the provided refresh token and issuing new tokens.
        public async Task<AuthResponse> Refresh(RefreshRequest request, string ip)
        {
            var hash = _jwtService.HashToken(request.RefreshToken);

            var token = await _repo.GetRefreshToken(hash);

            if (token == null || token.ExpiresAtUtc <= DateTime.UtcNow)
                throw new Exception("Invalid token");

            var newRefresh = _jwtService.GenerateRefreshToken();
            var newHash = _jwtService.HashToken(newRefresh);

            token.RevokedAtUtc = DateTime.UtcNow;

            await _repo.AddRefreshToken(new RefreshToken
            {
                UserId = token.UserId,
                TokenHash = newHash,
                ExpiresAtUtc = _jwtService.GetRefreshTokenExpiryUtc(),
                CreatedAtUtc = DateTime.UtcNow,
                CreatedByIp = ip
            });

            var accessToken = _jwtService.GenerateToken(token.User);

            await _repo.SaveChanges();

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = newRefresh,
                AccessTokenExpiresAtUtc = _jwtService.GetAccessTokenExpiryUtc()
            };
        }

        // Revokes a refresh token, preventing its future use for obtaining new access tokens.
        public async Task<string> Revoke(RefreshRequest request)
        {
            var hash = _jwtService.HashToken(request.RefreshToken);
            var token = await _repo.GetRefreshToken(hash);

            if (token == null)
                throw new Exception("Token not found");

            token.RevokedAtUtc = DateTime.UtcNow;
            await _repo.SaveChanges();

            return "Token revoked";
        }
    }
}
