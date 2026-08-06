namespace Ticketing.Application.DTOs.Users;

public class UpdateUserRequestDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public IReadOnlyList<string> Roles { get; set; } = [];
    public Guid? DepartmentId { get; set; }
}
