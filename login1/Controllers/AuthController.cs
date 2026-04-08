
    using login1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
    
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

        public AuthController(JwtService jwtService)
        {
            _jwtService = jwtService;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            // TEMP: Replace with DB check
            if (request.Username == "admin" && request.Password == "password")
            {
                var token = _jwtService.GenerateToken(request.Username);

                return Ok(new { token });
            }

            return Unauthorized("Invalid credentials");
        }
    }
