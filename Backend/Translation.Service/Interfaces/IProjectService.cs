using Translation.Contracts.DTO.Projects;

namespace Translation.Service.Interfaces
{
    // Service contract for project management workflows.
    public interface IProjectService
    {
        Task<List<ProjectDto>> GetProjectsAsync();
        Task<bool> UpdateProjectNameAsync(int projectId, string newName);
    }
}
