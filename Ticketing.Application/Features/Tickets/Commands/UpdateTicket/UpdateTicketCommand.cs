using MediatR;
using Ticketing.Application.DTOs;

namespace Ticketing.Application.Features.Tickets.Commands.UpdateTicket;

public record UpdateTicketCommand(Guid TicketId, UpdateTicketDto Request) : IRequest<TicketResponseDto>;
