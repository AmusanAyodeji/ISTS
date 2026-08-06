using MediatR;
using Ticketing.Application.DTOs;

namespace Ticketing.Application.Features.Tickets.Commands.AssignTicket;

public record AssignTicketCommand(Guid TicketId, AssignTicketRequestDto Request) : IRequest<TicketResponseDto>;
