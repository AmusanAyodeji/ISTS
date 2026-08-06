namespace Ticketing.Application.DTOs.Users;

public class CreateUserRequestDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public IReadOnlyList<string> Roles { get; set; } = [];
    public Guid? DepartmentId { get; set; }
}
