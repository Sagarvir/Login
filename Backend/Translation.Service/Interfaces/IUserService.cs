namespace Translation.Service.Interfaces
{
    // Service contract for user profile lookups.
    public interface IUserService
    {
        Task<String> GetUserNameByIdAsync(string employeeId);
    }
}