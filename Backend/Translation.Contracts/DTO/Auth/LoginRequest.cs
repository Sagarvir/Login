namespace Translation.Contracts.DTO.Auth
{
    // Request payload for user login.
    public class LoginRequest
    {
        public string? EmployeeId { get; set; }
        public string? Password { get; set; }
    }
}
