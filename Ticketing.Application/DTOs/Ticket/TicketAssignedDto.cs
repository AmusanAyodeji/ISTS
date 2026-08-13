namespace Ticketing.Application.DTOs;

public class TicketAssignedDto
{
    public Guid AgentId { get; set; }
    public Guid TicketId { get; set; }
}