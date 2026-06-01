using Microsoft.EntityFrameworkCore;
using Translation.DAO.Data;
using Translation.DAO.Repositories.Interfaces;
using Translation.Models.Entities;


namespace Translation.DAO.Repositories
{
    // Data access implementation for users, roles, and refresh tokens.
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        // Check if a user with the given employee ID already exists (case-insensitive).
        public async Task<bool> UserExists(string employeeId)
        {
            return await _context.Users
                .AnyAsync(u => u.EmployeeId.ToLower() == employeeId.ToLower());
        }

        // Retrieve a user by their employee ID, including their role and preferred language.   
        public async Task<User?> GetUserByEmployeeId(string employeeId)
        {
            return await _context.Users
                .Include(u => u.Role)
                .Include(u => u.PreferredLanguage)
                .FirstOrDefaultAsync(u => u.EmployeeId.ToLower() == employeeId.ToLower());
        }

        // Retrieve a role by its name (case-insensitive).
        public async Task<Role?> GetRoleByName(string roleName)
        {
            return await _context.Roles
                .FirstOrDefaultAsync(r => r.Name.ToLower() == roleName.ToLower());
        }

        // Retrieve the default role (assumed to be "viewer").
        public async Task<Role?> GetDefaultRole()
        {
            return await _context.Roles
                .FirstOrDefaultAsync(r => r.Name.ToLower() == "viewer");
        }

        // Retrieve a language by its ID.
        public async Task<Language?> GetLanguageById(int id)
        {
            return await _context.Languages.FirstOrDefaultAsync(l => l.Id == id);
        }

        // Add a new user to the database context.
        public async Task AddUser(User user)
        {
            await _context.Users.AddAsync(user);
        }

        // Save changes to the database.
        public async Task SaveChanges()
        {
            await _context.SaveChangesAsync();
        }

        // Retrieve a refresh token by its hash, including the associated user.
        public async Task<RefreshToken?> GetRefreshToken(string tokenHash)
        {
            return await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash);
        }

        // Add a new refresh token to the database context.
        public async Task AddRefreshToken(RefreshToken token)
        {
            await _context.RefreshTokens.AddAsync(token);
        }
    }
}
