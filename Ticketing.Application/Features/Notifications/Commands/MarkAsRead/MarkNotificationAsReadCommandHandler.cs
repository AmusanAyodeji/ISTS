using MediatR;
using Ticketing.Application.Interfaces.Persistence;

namespace Ticketing.Application.Features.Notifications.Commands.MarkAsRead;

public class MarkNotificationAsReadCommandHandler : IRequestHandler<MarkNotificationAsReadCommand, bool>
{
    private readonly INotificationRepository _notificationRepository;

    public MarkNotificationAsReadCommandHandler(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<bool> Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
    {
        var notification = await _notificationRepository.GetByIdAndUserIdAsync(
            request.NotificationId, request.UserId, cancellationToken);

        if (notification is null)
            throw new KeyNotFoundException($"Notification with ID '{request.NotificationId}' not found for this user.");

        if (notification.IsRead)
            return true;

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        _notificationRepository.Update(notification);
        await _notificationRepository.SaveChangesAsync(cancellationToken);

        return true;
    }
}