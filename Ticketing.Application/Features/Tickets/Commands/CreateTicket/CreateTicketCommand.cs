using MediatR;
using Ticketing.Application.DTOs;

namespace Ticketing.Application.Features.Tickets.Commands.CreateTicket;

public record CreateTicketCommand(CreateTicketDto Request, string? AttachmentUrl = null) : IRequest<TicketResponseDto>;
