namespace Translation.Contracts.DTO.Auth
{
    // Request payload for registering a new user.
    public class RegisterRequest
    {
        public string? EmployeeId { get; set; }
        public string? Password { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public int PreferredLanguageId { get; set; }
    }
}
