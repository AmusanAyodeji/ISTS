using Ticketing.Domain.Enums;

namespace Ticketing.Application.DTOs;

public class AssignTicketRequestDto
{
    public Guid AgentId { get; set; }
}

public class UpdateTicketStatusRequestDto
{
    public TicketStatus Status { get; set; }
}
