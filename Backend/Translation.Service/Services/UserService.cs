using Translation.DAO.Repositories.Interfaces;
using Translation.Service.Interfaces;

namespace Translation.Service.Services
{
    // Provides user profile lookups backed by the user repository.
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<String> GetUserNameByIdAsync(string employeeId)
        {
            if (string.IsNullOrWhiteSpace(employeeId))
                throw new Exception("Employee ID is required");

            var user = await _userRepository.GetUserByEmployeeId(employeeId);
            if (user == null)
                throw new Exception("User not found");

            if (user.FirstName == null || user.LastName == null)
                throw new Exception("The user name is not found in the database");

            return user.FirstName + " " + user.LastName;
        }
    }
}