using MediatR;
using Ticketing.Application.DTOs;

namespace Ticketing.Application.Features.Tickets.Commands.UpdateTicketStatus;

public record UpdateTicketStatusCommand(Guid TicketId, UpdateTicketStatusRequestDto Request) : IRequest<TicketResponseDto>;
