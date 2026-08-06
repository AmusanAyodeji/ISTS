using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Ticketing.Application.DTOs.Notifications;
using Ticketing.Application.Interfaces.Persistence;
using Ticketing.Application.Interfaces.Services;
using Ticketing.Domain.Entities;
using Ticketing.Domain.Enums;

namespace Ticketing.Infrastructure.BackgroundJobs;

public class SLABreachBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SLABreachBackgroundService> _logger;

    public SLABreachBackgroundService(IServiceProvider serviceProvider, ILogger<SLABreachBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SLA breach background service started.");

        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(5));

        do
        {
            try
            {
                await CheckBreachesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check SLA breaches.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task CheckBreachesAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var ticketRepository = scope.ServiceProvider.GetRequiredService<ITicketRepository>();
        var notificationRepository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var slaRepository = scope.ServiceProvider.GetRequiredService<ISLARepository>();
        var notificationHubService = scope.ServiceProvider.GetRequiredService<INotificationHubService>();

        var now = DateTime.UtcNow;
        var breachedCount = 0;

        _logger.LogInformation("SLA breach check is running...");

        var unresolvedTickets = await ticketRepository.GetUnresolvedTickets(cancellationToken);
        foreach (var ticket in unresolvedTickets)
        {
            var slaRule = await slaRepository.GetSLAByPriority(ticket.DepartmentId, ticket.Priority, cancellationToken);
            if (slaRule is null)
            {
                _logger.LogWarning(
                    "No SLA rule found for ticket {TicketId} (department {DepartmentId}, priority {Priority}).",
                    ticket.Id,
                    ticket.DepartmentId,
                    ticket.Priority);
                continue;
            }

            var responseDeadline = ticket.CreatedAt + TimeSpan.FromMinutes(slaRule.ResponseTimeMinutes);
            var resolutionDeadline = ticket.CreatedAt + TimeSpan.FromMinutes(slaRule.ResolutionTimeMinutes);

            bool isBreached = ticket.Status == TicketStatus.Open && now >= responseDeadline
                || ticket.Status == TicketStatus.InProgress && now >= resolutionDeadline;

            if (!ticket.SLABreached && isBreached)
            {
                ticket.SLABreached = true;
                breachedCount++;
            }
        }

        if (breachedCount > 0)
        {
            await ticketRepository.SaveChangesAsync(cancellationToken);
        }

        _logger.LogInformation(
            "SLA breach check completed at {Time}. Breached tickets updated: {Count}",
            now,
            breachedCount);

        var activeTickets = await ticketRepository.GetActiveTicketsAsync(cancellationToken);
        foreach (var ticket in activeTickets)
        {
            if (!ticket.SlaDueAt.HasValue || now <= ticket.SlaDueAt.Value)
            {
                continue;
            }

            var overdueBy = now - ticket.SlaDueAt.Value;
            _logger.LogInformation(
                "Ticket {TicketId} is overdue by {OverdueBy}.",
                ticket.Id,
                overdueBy);

            var message = $"Ticket '{ticket.Title}' is overdue by {FormatOverdue(overdueBy)}.";
            var targetUserIds = new HashSet<Guid>();

            if (ticket.AssignedToId.HasValue)
            {
                targetUserIds.Add(ticket.AssignedToId.Value);
            }

            var managers = await userRepository.GetManagersByDepartmentAsync(ticket.DepartmentId, cancellationToken);
            foreach (var manager in managers)
            {
                targetUserIds.Add(manager.Id);
            }

            foreach (var userId in targetUserIds)
            {
                bool alreadyNotified = await notificationRepository.HasBreachNotificationForTicketAsync(userId, ticket.Id, cancellationToken);
                if (alreadyNotified)
                {
                    continue;
                }

                var notification = new Notification
                {
                    UserId = userId,
                    Title = "SLA Breach Alert",
                    Message = message,
                    Type = NotificationType.Warning,
                    TicketId = ticket.Id
                };

                await notificationRepository.AddAsync(notification, cancellationToken);

                await notificationHubService.NotifyUserAsync(userId, new NotificationDto
                {
                    Id = notification.Id,
                    Title = notification.Title,
                    Message = notification.Message,
                    Type = notification.Type,
                    IsRead = notification.IsRead,
                    ReadAt = notification.ReadAt,
                    TicketId = notification.TicketId,
                    CreatedAt = notification.CreatedAt
                });
            }
        }

        await ticketRepository.SaveChangesAsync(cancellationToken);
    }

    private static string FormatOverdue(TimeSpan overdue)
    {
        if (overdue.TotalDays >= 1)
            return $"{overdue.TotalDays:F0} day{(overdue.TotalDays >= 2 ? "s" : "")}";
        if (overdue.TotalHours >= 1)
            return $"{overdue.TotalHours:F0} hr{(overdue.TotalHours >= 2 ? "s" : "")}";
        return $"{overdue.TotalMinutes:F0} min{(overdue.TotalMinutes >= 2 ? "s" : "")}";
    }
}
