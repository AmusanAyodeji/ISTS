using Ticketing.Application.DTOs.Messages;
using Ticketing.Application.DTOs.Notifications;

namespace Ticketing.Application.Interfaces.Services;

public interface INotificationHubService
{
    Task SendMessageToTicketGroupAsync(Guid ticketId, TicketMessageDto message);
    Task NotifyUserAsync(Guid userId, NotificationDto notification);
    Task SendTypingIndicatorAsync(Guid ticketId, Guid userId, string userName);
    Task SendReadReceiptAsync(Guid ticketId, Guid userId);
}