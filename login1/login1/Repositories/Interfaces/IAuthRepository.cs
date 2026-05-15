using login1.Models;

namespace TranslationService.Repositories.Interfaces
{
    public interface IAuthRepository
    {
        Task<bool> UserExists(string employeeId);
        Task<User?> GetUserByEmployeeId(string employeeId);
        Task<Role?> GetRoleByName(string roleName);
        Task<Role?> GetDefaultRole();
        Task<Language?> GetLanguageById(int id);

        Task AddUser(User user);
        Task SaveChanges();

        Task<RefreshToken?> GetRefreshToken(string tokenHash);
        Task AddRefreshToken(RefreshToken token);
    }
}
