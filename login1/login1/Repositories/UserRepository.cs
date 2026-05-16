using login1.Data;
using login1.Models;
using Microsoft.EntityFrameworkCore;
using TranslationService.Repositories.Interfaces;

namespace TranslationService.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> UserExists(string employeeId)
        {
            return await _context.Users
                .AnyAsync(u => u.EmployeeId.ToLower() == employeeId.ToLower());
        }

        public async Task<User?> GetUserByEmployeeId(string employeeId)
        {
            return await _context.Users
                .Include(u => u.Role)
                .Include(u => u.PreferredLanguage)
                .FirstOrDefaultAsync(u => u.EmployeeId.ToLower() == employeeId.ToLower());
        }

        public async Task<Role?> GetRoleByName(string roleName)
        {
            return await _context.Roles
                .FirstOrDefaultAsync(r => r.Name.ToLower() == roleName.ToLower());
        }

        public async Task<Role?> GetDefaultRole()
        {
            return await _context.Roles
                .FirstOrDefaultAsync(r => r.Name.ToLower() == "viewer");
        }

        public async Task<Language?> GetLanguageById(int id)
        {
            return await _context.Languages.FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task AddUser(User user)
        {
            await _context.Users.AddAsync(user);
        }

        public async Task SaveChanges()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<RefreshToken?> GetRefreshToken(string tokenHash)
        {
            return await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash);
        }

        public async Task AddRefreshToken(RefreshToken token)
        {
            await _context.RefreshTokens.AddAsync(token);
        }
    }
}
