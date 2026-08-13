using Microsoft.AspNetCore.SignalR;
using Ticketing.Application.DTOs.Messages;
using Ticketing.Application.DTOs.Notifications;
using Ticketing.Application.Interfaces.Services;
using Ticketing.Application.DTOs;

namespace Ticketing.Infrastructure.Realtime;

public class NotificationHubService : INotificationHubService
{
    private readonly IHubContext<SupportHub> _supportHubContext;
    private readonly IHubContext<NotificationHub> _notificationHubContext;

    public NotificationHubService(
        IHubContext<SupportHub> supportHubContext,
        IHubContext<NotificationHub> notificationHubContext)
    {
        _supportHubContext = supportHubContext;
        _notificationHubContext = notificationHubContext;
    }

    public async Task SendMessageToTicketGroupAsync(Guid ticketId, TicketMessageDto message)
    {
        await _supportHubContext.Clients.Group($"ticket-{ticketId}")
            .SendAsync("ReceiveMessage", message);
    }

    public async Task NotifyUserAsync(Guid userId, NotificationDto notification)
    {
        await _notificationHubContext.Clients.User(userId.ToString())
            .SendAsync("Notification", notification);
    }

    public async Task SendTypingIndicatorAsync(Guid ticketId, Guid userId, string userName)
    {
        await _supportHubContext.Clients.Group($"ticket-{ticketId}")
            .SendAsync("Typing", new { UserId = userId, UserName = userName, TicketId = ticketId });
    }

    public async Task SendReadReceiptAsync(Guid ticketId, Guid userId)
    {
        await _supportHubContext.Clients.Group($"ticket-{ticketId}")
            .SendAsync("ReadReceipt", new { UserId = userId, TicketId = ticketId });
    }
    public async Task NotifyTicketStatusChangedAsync(Guid ticketId, TicketResponseDto ticket)
    {
        await _supportHubContext
            .Clients
            .Group($"ticket-{ticketId}")
            .SendAsync("TicketStatusChanged", ticket);
    }
    public async Task NotifyUnassignedTicketAcceptedAsync(TicketAssignedDto dto)
    {
        await _supportHubContext
            .Clients
            .Group("ticketqueue")
            .SendAsync("TicketAccepted", dto);
    }
    public async Task AddTicketToQueueAsync(TicketResponseDto ticket)
    {
        await _supportHubContext
            .Clients
            .Group("ticketqueue")
            .SendAsync("TicketAdded", ticket);
    }
    public async Task NotifyTicketDeletionAsync(Guid TicketId)
    {
        await _supportHubContext
            .Clients
            .Group("ticketqueue")
            .SendAsync("TicketDeleted", TicketId);
    }
}