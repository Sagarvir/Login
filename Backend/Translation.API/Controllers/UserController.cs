using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Translation.Service.Interfaces;

namespace Translation.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("{employeeId}/user-name")]
        [Authorize(Roles = "Admin,Creator,Translator,Viewer")]
        public async Task<IActionResult> Getuserbyid(string employeeId)
        {
            try
            {
                var username = await _userService.GetUserNameByIdAsync(employeeId);
                return Ok(username);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }


    }
}