using MediatR;

namespace Ticketing.Application.Features.Tickets.Commands.DeleteTicket;

public record DeleteTicketCommand(Guid TicketId) : IRequest<Unit>;
