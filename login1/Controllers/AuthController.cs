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
    public async Task<IActionResult> Register(LoginRequest request)
    {
        // Check if user exists
        var exists = await _context.Users
            .AnyAsync(u => u.Username == request.Username);

        if (exists)
            return BadRequest("User already exists");

        var user = new User
        {
            Username = request.Username,
            Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role_id = request.Role_id
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok("User registered successfully");
    }
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == request.Username);
        
        if (user == null)
            return Unauthorized("Invalid username");

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
