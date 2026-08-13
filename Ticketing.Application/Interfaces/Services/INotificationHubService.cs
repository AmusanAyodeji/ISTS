using Ticketing.Application.DTOs.Messages;
using Ticketing.Application.DTOs.Notifications;
using Ticketing.Application.DTOs;


namespace Ticketing.Application.Interfaces.Services;

public interface INotificationHubService
{
    Task SendMessageToTicketGroupAsync(Guid ticketId, TicketMessageDto message);
    Task NotifyUserAsync(Guid userId, NotificationDto notification);
    Task SendTypingIndicatorAsync(Guid ticketId, Guid userId, string userName);
    Task SendReadReceiptAsync(Guid ticketId, Guid userId);
    Task NotifyTicketStatusChangedAsync(Guid ticketId, TicketResponseDto ticket);
    Task NotifyUnassignedTicketAcceptedAsync(TicketAssignedDto dto);
    Task AddTicketToQueueAsync(TicketResponseDto ticket);
    Task NotifyTicketDeletionAsync(Guid TicketId);
}