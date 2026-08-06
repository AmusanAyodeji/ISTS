using Ticketing.Domain.Enums;
namespace Ticketing.Application.DTOs;

public class CreateTicketDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid DepartmentId { get; set; }
     public Guid CategoryId { get; set; }

    public TicketPriority Priority { get; set; }
}