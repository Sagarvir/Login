using Translation.Contracts.DTO.Projects;
using Translation.DAO.Repositories.Interfaces;
using Translation.Service.Interfaces;

namespace Translation.Service.Services
{
    // Implements project management workflows.
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _repo;

        // Constructor injects the project repository for data access.
        public ProjectService(IProjectRepository repo)
        {
            _repo = repo;
        }

        // Retrieves all projects and maps them to DTOs.
        public async Task<List<ProjectDto>> GetProjectsAsync()
        {
            var projects = await _repo.GetProjectsAsync();
            return projects.Select(p => new ProjectDto
            {
                Id = p.Id,
                Name = p.Name
            }).ToList();
        }
    }
}
