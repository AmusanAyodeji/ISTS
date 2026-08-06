

namespace Ticketing.Application.DTOs.Department;

public class DepartmentResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public IReadOnlyList<CategoryResponseDto> Categories { get; set; } = [];
}
