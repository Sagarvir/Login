namespace Translation.Contracts.DTO.Auth
{
    // Request payload for assigning a role to a user.
    public class AssignRoleRequest
    {
        public string EmployeeId { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
    }
}
