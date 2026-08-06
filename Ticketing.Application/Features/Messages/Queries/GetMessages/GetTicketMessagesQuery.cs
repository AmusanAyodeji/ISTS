using MediatR;
using Ticketing.Application.DTOs.Messages;

namespace Ticketing.Application.Features.Messages.Queries.GetMessages;

public record GetTicketMessagesQuery(
    Guid TicketId,
    Guid RequestingUserId) : IRequest<IReadOnlyList<TicketMessageDto>>;