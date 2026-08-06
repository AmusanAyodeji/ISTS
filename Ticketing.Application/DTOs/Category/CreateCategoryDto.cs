namespace Ticketing.Application.DTOs;
public class CreateCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public Guid DepartmentId { get; set; }
}