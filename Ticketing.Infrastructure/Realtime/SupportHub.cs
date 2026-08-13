using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Ticketing.Application.Interfaces.Services;

namespace Ticketing.Infrastructure.Realtime;

[Authorize]
public class SupportHub : Hub
{
    private readonly INotificationHubService _notificationHubService;

    public SupportHub(INotificationHubService notificationHubService)
    {
        _notificationHubService = notificationHubService;
    }

    public async Task JoinTicketGroup(Guid ticketId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"ticket-{ticketId}");
    }

    public async Task LeaveTicketGroup(Guid ticketId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"ticket-{ticketId}");
    }

    public async Task JoinUnassignedTicketsGroup(Guid ticketId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "ticketqueue");
    }

    public async Task LeaveUnassignedTicketsGroup(Guid ticketId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "ticketqueue");
    }

    public async Task Typing(Guid ticketId)
    {
        var userName = Context.User?.Identity?.Name ?? "Unknown";
        var userId = Context.UserIdentifier;
        await _notificationHubService.SendTypingIndicatorAsync(ticketId, Guid.Parse(userId ?? Guid.Empty.ToString()), userName);
    }

    public async Task ReadReceipt(Guid ticketId)
    {
        var userId = Context.UserIdentifier;
        if (userId is not null)
            await _notificationHubService.SendReadReceiptAsync(ticketId, Guid.Parse(userId));
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (userId is not null)
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        if (userId is not null)
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user-{userId}");

        await base.OnDisconnectedAsync(exception);
    }
}