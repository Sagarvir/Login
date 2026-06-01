using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Translation.Contracts.DTO.Auth;
using Translation.Service.Interfaces;
namespace Translation.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _service;

        // Constructor injects the authentication service for handling auth workflows.
        public AuthController(IAuthService service)
        {
            _service = service;
        }

        // Register a new user and persist credentials.
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            try
            {

                string? result = await _service.Register(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Authenticate a user and issue access/refresh tokens.
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            try
            {
                return Ok(await _service.Login(request));
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        // Assign a role to an existing user (Admin only).
        [Authorize(Roles = "Admin")]
        [HttpPut("assign-role")]
        public async Task<IActionResult> AssignRole(AssignRoleRequest request)
        {
            try
            {
                return Ok(await _service.AssignRole(request));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Exchange a refresh token for a new access token.
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(RefreshRequest request)
        {
            try
            {
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                return Ok(await _service.Refresh(request, ip));
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        // Revoke a refresh token to end the session.
        [HttpPost("revoke")]
        public async Task<IActionResult> Revoke(RefreshRequest request)
        {
            try
            {
                return Ok(await _service.Revoke(request));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}