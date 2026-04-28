using login1.Data;
using login1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    [Authorize]   // 🔐 ONLY this endpoint is protected
    [HttpGet("secure")]
    public IActionResult Secure()
    {
        return Ok("This is protected data");
    }
    private readonly JwtService _jwtService;
    private readonly AppDbContext _context;

    public AuthController(JwtService jwtService, AppDbContext context)
    {
        _jwtService = jwtService;
        _context = context;
    }
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        // Try to be resilient to different frontends: if required fields are missing
        // try parsing the raw JSON body case-insensitively (helps when property names differ).
        if ((request == null) || string.IsNullOrWhiteSpace(request.EmployeeId) || string.IsNullOrWhiteSpace(request.Password)
            || string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
        {
            try
            {
                Request.EnableBuffering();
                Request.Body.Position = 0;
                using var doc = await System.Text.Json.JsonDocument.ParseAsync(Request.Body);
                var root = doc.RootElement;
                string? ReadAny(params string[] names)
                {
                    foreach (var n in names)
                    {
                        if (root.TryGetProperty(n, out var prop) && prop.ValueKind != System.Text.Json.JsonValueKind.Null)
                            return prop.GetString();
                        // also try lower/upper variants
                        if (root.TryGetProperty(n.ToLower(), out prop) && prop.ValueKind != System.Text.Json.JsonValueKind.Null)
                            return prop.GetString();
                        if (root.TryGetProperty(n.ToUpper(), out prop) && prop.ValueKind != System.Text.Json.JsonValueKind.Null)
                            return prop.GetString();
                    }
                    return null;
                }

                request ??= new RegisterRequest();
                request.EmployeeId ??= ReadAny("employeeId", "EmployeeId", "username", "user", "empId", "employee_id");
                request.Password ??= ReadAny("password", "Password", "pwd");
                request.FirstName ??= ReadAny("firstName", "FirstName", "first_name", "fname");
                request.LastName ??= ReadAny("lastName", "LastName", "last_name", "lname");
                request.PreferredLanguage ??= ReadAny("preferredLanguage", "PreferredLanguage", "language", "lang");
                Request.Body.Position = 0;
            }
            catch
            {
                // ignore parsing errors and continue with model-bound values
            }
        }
        // Support both JSON and form-posted submissions from different frontends.
        if (request == null)
        {
            if (Request.HasFormContentType)
            {
                var form = await Request.ReadFormAsync();
                request = new RegisterRequest
                {
                    EmployeeId = form["employeeId"].FirstOrDefault() ?? form["EmployeeId"].FirstOrDefault(),
                    Password = form["password"].FirstOrDefault() ?? form["Password"].FirstOrDefault(),
                    FirstName = form["firstName"].FirstOrDefault() ?? form["FirstName"].FirstOrDefault(),
                    LastName = form["lastName"].FirstOrDefault() ?? form["LastName"].FirstOrDefault(),
                    PreferredLanguage = form["preferredLanguage"].FirstOrDefault() ?? form["PreferredLanguage"].FirstOrDefault()
                };
            }
        }
        // normalize and validate inputs (trim + lowercase employee id & language)
        var emp = request?.EmployeeId?.Trim().ToLower();
        var pwd = request?.Password?.Trim();
        var fname = request?.FirstName?.Trim();
        var lname = request?.LastName?.Trim();
        var preferred = request?.PreferredLanguage?.Trim().ToLower() ?? "english";

        var missing = new List<string>();
        if (string.IsNullOrWhiteSpace(emp)) missing.Add("EmployeeId");
        if (string.IsNullOrWhiteSpace(pwd)) missing.Add("Password");
        if (string.IsNullOrWhiteSpace(fname)) missing.Add("FirstName");
        if (string.IsNullOrWhiteSpace(lname)) missing.Add("LastName");

        if (missing.Count > 0)
            return BadRequest($"Missing required fields: {string.Join(", ", missing)}");

        // Check if employee already exists (case-insensitive)
        var exists = await _context.Users
            .AnyAsync(u => u.EmployeeId.ToLower() == emp);

        if (exists)
            return BadRequest("Employee already registered");

        var user = new User
        {
            EmployeeId = emp,
            FirstName = fname!,
            LastName = lname!,
            Password = BCrypt.Net.BCrypt.HashPassword(pwd!),
            RoleId = null, // Role will be assigned by admin later
            PreferredLanguage = string.IsNullOrWhiteSpace(preferred) ? "english" : preferred
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok("User registered successfully");
    }
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.EmployeeId == request.EmployeeId);

        if (user == null)
            return Unauthorized("Invalid employee ID");

        // 🔐 Compare hashed password
        bool isValid = BCrypt.Net.BCrypt.Verify(
            request.Password,
            user.Password
        );

        if (!isValid)
            return Unauthorized("Invalid password");

        var accessToken = _jwtService.GenerateToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();
        var refreshTokenHash = _jwtService.HashToken(refreshToken);

        _context.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = refreshTokenHash,
            ExpiresAtUtc = _jwtService.GetRefreshTokenExpiryUtc(),
            CreatedAtUtc = DateTime.UtcNow,
            CreatedByIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"
        });

        await _context.SaveChangesAsync();

        return Ok(new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiresAtUtc = _jwtService.GetAccessTokenExpiryUtc()
        });
    }
    [Authorize(Roles = "Admin")]
    [HttpPut("assign-role")]
    public async Task<IActionResult> AssignRole(AssignRoleRequest request)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(request.EmployeeId) || string.IsNullOrWhiteSpace(request.RoleName))
            return BadRequest("EmployeeId and RoleName are required");

        // Find user by EmployeeId
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.EmployeeId.ToLower() == request.EmployeeId.ToLower());

        if (user == null)
            return NotFound($"User with EmployeeId '{request.EmployeeId}' not found");

        // Find role by name
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name.ToLower() == request.RoleName.ToLower());

        if (role == null)
            return NotFound($"Role '{request.RoleName}' not found");

        // Assign role
        user.RoleId = role.Id;
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Role assigned successfully",
            employeeId = user.EmployeeId,
            firstName = user.FirstName,
            lastName = user.LastName,
            assignedRole = role.Name
        });
    }
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshRequest request)
    {
        var tokenHash = _jwtService.HashToken(request.RefreshToken);

        var storedToken = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash);

        if (storedToken == null)
            return Unauthorized("Invalid refresh token");

        if (storedToken.RevokedAtUtc != null || storedToken.ExpiresAtUtc <= DateTime.UtcNow)
            return Unauthorized("Refresh token is no longer active");

        var newRefreshToken = _jwtService.GenerateRefreshToken();
        var newRefreshTokenHash = _jwtService.HashToken(newRefreshToken);

        storedToken.RevokedAtUtc = DateTime.UtcNow;
        storedToken.ReplacedByTokenHash = newRefreshTokenHash;

        _context.RefreshTokens.Add(new RefreshToken
        {
            UserId = storedToken.UserId,
            TokenHash = newRefreshTokenHash,
            ExpiresAtUtc = _jwtService.GetRefreshTokenExpiryUtc(),
            CreatedAtUtc = DateTime.UtcNow,
            CreatedByIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"
        });

        var accessToken = _jwtService.GenerateToken(storedToken.User);
        await _context.SaveChangesAsync();

        return Ok(new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
            AccessTokenExpiresAtUtc = _jwtService.GetAccessTokenExpiryUtc()
        });
    }

    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke(RefreshRequest request)
    {
        var tokenHash = _jwtService.HashToken(request.RefreshToken);
        var storedToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash);

        if (storedToken == null)
            return NotFound("Refresh token not found");

        if (storedToken.RevokedAtUtc != null)
            return BadRequest("Refresh token already revoked");

        storedToken.RevokedAtUtc = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok("Refresh token revoked");
    }
}
