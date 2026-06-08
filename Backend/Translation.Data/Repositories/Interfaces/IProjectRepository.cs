using Translation.Models.Entities;

namespace Translation.DAO.Repositories.Interfaces
{
    // Repository contract for project persistence.
    public interface IProjectRepository
    {
        Task<List<Project>> GetProjectsAsync();
        Task<Project?> GetProjectByIdAsync(int id);
        Task SaveChangesAsync();

        Task UpdateProjectAsync(int projectId, string newName);
    }
}
