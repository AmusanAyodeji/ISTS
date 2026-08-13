using MediatR;
using Ticketing.Application.DTOs;

namespace Ticketing.Application.Features.Tickets.Commands.CloseTicket;

public record CloseTicketCommand(Guid TicketId) : IRequest<Unit>;
