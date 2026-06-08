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

        [HttpPut]
        [Authorize(Roles = "Admin,Creator")]
        public async Task<IActionResult> UpdateProjectName(int projectId, string newName)
        {
            try
            {
                var result = await _projectService.UpdateProjectNameAsync(projectId, newName);
                if (result)
                {
                    return Ok(new { success = true, message = "Project name updated successfully." });
                }
                else
                {
                    return NotFound(new { success = false, message = "Project not found." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while updating the project name.", error = ex.Message });
            }
        }
    }
}
