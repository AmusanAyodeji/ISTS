using MediatR;
using Ticketing.Application.DTOs;

namespace Ticketing.Application.Features.Tickets.Commands.EscalateTicket;

public record EscalateTicketCommand(Guid TicketId) : IRequest<TicketResponseDto>;
