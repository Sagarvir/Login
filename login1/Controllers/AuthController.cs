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

        var token = _jwtService.GenerateToken(user);

        return Ok(new { token });
    }
}
