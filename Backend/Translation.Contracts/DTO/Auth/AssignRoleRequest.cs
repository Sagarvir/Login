namespace Translation.Contracts.DTO.Auth
{
    public class AssignRoleRequest
    {
        public string EmployeeId { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
    }
}
