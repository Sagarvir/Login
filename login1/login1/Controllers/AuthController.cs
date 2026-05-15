using login1.Data;
using login1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TranslationService.Services.Interfaces;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _service;

    public AuthController(IAuthService service)
    {
        _service = service;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        try
        {
            var result = await _service.Register(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

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