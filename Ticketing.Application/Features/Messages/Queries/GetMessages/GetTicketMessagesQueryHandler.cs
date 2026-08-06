using MediatR;
using Ticketing.Application.DTOs.Messages;
using Ticketing.Application.Interfaces.Persistence;

namespace Ticketing.Application.Features.Messages.Queries.GetMessages;

public class GetTicketMessagesQueryHandler : IRequestHandler<GetTicketMessagesQuery, IReadOnlyList<TicketMessageDto>>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly ITicketMessageRepository _messageRepository;

    public GetTicketMessagesQueryHandler(
        ITicketRepository ticketRepository,
        ITicketMessageRepository messageRepository)
    {
        _ticketRepository = ticketRepository;
        _messageRepository = messageRepository;
    }

    public async Task<IReadOnlyList<TicketMessageDto>> Handle(GetTicketMessagesQuery request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken);
        if (ticket is null)
            throw new KeyNotFoundException($"Ticket with ID '{request.TicketId}' not found.");

        var messages = await _messageRepository.GetByTicketIdAsync(request.TicketId, cancellationToken);

        return messages.Select(m => new TicketMessageDto
        {
            Id = m.Id,
            TicketId = m.TicketId,
            SenderUserId = m.SenderUserId,
            SenderName = $"{m.SenderUser.FirstName} {m.SenderUser.LastName}",
            Message = m.Message,
            IsInternal = m.IsInternal,
            AttachmentUrl = m.AttachmentUrl,
            CreatedAt = m.CreatedAt
        }).ToList();
    }
}