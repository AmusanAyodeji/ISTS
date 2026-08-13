using MediatR;
using Ticketing.Application.DTOs;

namespace Ticketing.Application.Features.Tickets.Commands.ReassignTicket;

public record ReassignTicketCommand(Guid TicketId, AssignTicketRequestDto Request) : IRequest<TicketResponseDto>;
