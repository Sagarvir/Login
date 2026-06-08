using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Translation.Service.Interfaces;

namespace Translation.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService _projectService;

        public ProjectController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        // GET all projects with their names and IDs
        [HttpGet]
        [Authorize(Roles = "Admin,Creator,Translator,Viewer")]
        public async Task<IActionResult> GetProjects()
        {
            try
            {
                var projects = await _projectService.GetProjectsAsync();
                return Ok(projects);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving projects.", error = ex.Message });
            }
        }
    }
}
