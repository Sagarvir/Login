using Microsoft.EntityFrameworkCore;
using Translation.DAO.Data;
using Translation.DAO.Repositories.Interfaces;
using Translation.Models.Entities;

namespace Translation.DAO.Repositories
{
    // Data access implementation for projects.
    public class ProjectRepository : IProjectRepository
    {
        private readonly AppDbContext _context;

        public ProjectRepository(AppDbContext context)
        {
            _context = context;
        }

        // Retrieves all projects from the database.
        public async Task<List<Project>> GetProjectsAsync()
        {
            return await _context.Projects.ToListAsync();
        }

        // Retrieves a project by its ID.
        public async Task<Project?> GetProjectByIdAsync(int id)
        {
            return await _context.Projects.FindAsync(id);
        }

        // Saves changes to the database.
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
