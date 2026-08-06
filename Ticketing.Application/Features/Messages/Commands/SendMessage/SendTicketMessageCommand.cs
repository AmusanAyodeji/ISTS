using MediatR;
using Ticketing.Application.DTOs.Messages;

namespace Ticketing.Application.Features.Messages.Commands.SendMessage;

public record SendTicketMessageCommand(
    Guid TicketId,
    string Message,
    bool IsInternal,
    string? AttachmentUrl,
    Guid SenderUserId) : IRequest<TicketMessageDto>;