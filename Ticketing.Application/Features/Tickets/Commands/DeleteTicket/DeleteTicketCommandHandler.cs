using MediatR;
using Ticketing.Application.Interfaces.Persistence;
using Ticketing.Application.Interfaces.Services;

namespace Ticketing.Application.Features.Tickets.Commands.DeleteTicket;

public class DeleteTicketCommandHandler : IRequestHandler<DeleteTicketCommand, Unit>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly ITicketMessageRepository _messageRepository;
    private readonly IRatingRepository _ratingRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly INotificationHubService _notificationHubService;

    public DeleteTicketCommandHandler(
        ITicketRepository ticketRepository,
        ITicketMessageRepository messageRepository,
        IRatingRepository ratingRepository,
        INotificationRepository notificationRepository,
        ICurrentUserService currentUserService,
        INotificationHubService notificationHubService)
    {
        _ticketRepository = ticketRepository;
        _messageRepository = messageRepository;
        _ratingRepository = ratingRepository;
        _notificationRepository = notificationRepository;
        _currentUserService = currentUserService;
        _notificationHubService = notificationHubService;
    }

    public async Task<Unit> Handle(DeleteTicketCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User must be authenticated to delete a ticket.");
        }

        var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken)
            ?? throw new InvalidOperationException("Ticket not found.");

        if (ticket.CreatedById != _currentUserService.UserId.Value)
        {
            throw new UnauthorizedAccessException("You can only delete tickets you created.");
        }

        // Load related ratings/notification entities without their navigation graph so we do
        // not end up tracking the same User instances twice. Deleting the principal (Ticket)
        // cascades to TicketMessages automatically. Ratings are cascaded by EF configuration,
        // but we delete explicitly to avoid relying on it. Notifications have Restrict delete
        // behavior, so they must be removed first.
        var ratings = await _ratingRepository.ListByTicketIdAsync(request.TicketId, cancellationToken);
        foreach (var rating in ratings)
        {
            _ratingRepository.Delete(rating);
        }

        var notifications = await _notificationRepository.GetByTicketIdAsync(request.TicketId, cancellationToken);
        foreach (var notification in notifications)
        {
            _notificationRepository.Delete(notification);
        }

        var messages = await _messageRepository.GetByTicketIdNoTrackingAsync(request.TicketId, cancellationToken);
        foreach (var message in messages)
        {
            _messageRepository.Delete(message);
        }

        _ticketRepository.Delete(ticket);
        await _ticketRepository.SaveChangesAsync(cancellationToken);
        await _notificationHubService.NotifyTicketDeletionAsync(request.TicketId);
        return Unit.Value;
    }
}
