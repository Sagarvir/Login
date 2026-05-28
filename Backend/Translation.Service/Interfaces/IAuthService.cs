using Translation.Contracts.DTO.Auth;

namespace Translation.Service.Interfaces
{
    public interface IAuthService
    {
        Task<string> Register(RegisterRequest request);
        Task<AuthResponse> Login(LoginRequest request);
        Task<object> AssignRole(AssignRoleRequest request);
        Task<AuthResponse> Refresh(RefreshRequest request, string ipAddress);
        Task<string> Revoke(RefreshRequest request);
    }
}
