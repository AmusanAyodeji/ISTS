using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Ticketing.Application.Common.Mappings;
using Ticketing.Application.DTOs;
using Ticketing.Application.Interfaces.Persistence;
using Ticketing.Domain.Entities;
using Ticketing.Domain.Enums;

namespace Ticketing.Application.Features.Tickets.Queries.GetTicketAnalytics;

public class GetTicketAnalyticsQueryHandler : IRequestHandler<GetTicketAnalyticsQuery, TicketAnalyticsDto>
{
    private readonly ITicketRepository _ticketRepository;

    public GetTicketAnalyticsQueryHandler(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<TicketAnalyticsDto> Handle(GetTicketAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var tickets = await _ticketRepository.GetFilteredAsync(
            null, null, null, null, request.FromDate, request.ToDate, cancellationToken);

        var now = DateTime.UtcNow;
        var startOfWeek = now.AddDays(-(int)now.DayOfWeek).Date;

        var weeklyVolume = Enumerable.Range(0, 7)
            .Select(i => startOfWeek.AddDays(i))
            .Select(date => new WeeklyTicketPoint
            {
                Label = date.ToString("ddd"),
                Received = tickets.Count(t => t.CreatedAt.Date == date.Date),
                Resolved = tickets.Count(t => t.ResolvedAt.HasValue && t.ResolvedAt.Value.Date == date.Date)
            })
            .ToList();

        var statusDistribution = new List<StatusDistributionSegment>
        {
            new() { Label = TicketStatus.Open.GetDisplayName(), Value = tickets.Count(t => t.Status == TicketStatus.Open), Color = "#2559AA" },
            new() { Label = TicketStatus.InProgress.GetDisplayName(), Value = tickets.Count(t => t.Status == TicketStatus.InProgress), Color = "#F59E0B" },
            new() { Label = TicketStatus.Resolved.GetDisplayName(), Value = tickets.Count(t => t.Status == TicketStatus.Resolved || t.Status == TicketStatus.Closed), Color = "#16A34A" }
        };

        var priorityDistribution = new List<PriorityDistributionSegment>
        {
            new() { Label = TicketPriority.Low.GetDisplayName(), Value = tickets.Count(t => t.Priority == TicketPriority.Low), Color = "#10B981" },
            new() { Label = TicketPriority.Medium.GetDisplayName(), Value = tickets.Count(t => t.Priority == TicketPriority.Medium), Color = "#3B82F6" },
            new() { Label = TicketPriority.High.GetDisplayName(), Value = tickets.Count(t => t.Priority == TicketPriority.High), Color = "#F59E0B" },
            new() { Label = TicketPriority.Urgent.GetDisplayName(), Value = tickets.Count(t => t.Priority == TicketPriority.Urgent), Color = "#DC2626" }
        };

        var agentWorkload = tickets
            .Where(t => t.AssignedToId.HasValue)
            .GroupBy(t => new { t.AssignedToId, Name = $"{t.AssignedTo!.FirstName} {t.AssignedTo.LastName}" })
            .Select(g => new AgentWorkloadEntry
            {
                Name = g.Key.Name,
                Initials = string.Concat(g.Key.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(n => n[0])).ToUpperInvariant(),
                Open = g.Count(t => t.Status == TicketStatus.Open || t.Status == TicketStatus.InProgress),
                Resolved = g.Count(t => t.Status == TicketStatus.Resolved || t.Status == TicketStatus.Closed)
            })
            .OrderByDescending(a => a.Open)
            .ToList();

        var ticketsWithSla = tickets.Where(t => t.SlaDueAt.HasValue).ToList();
        var breachedTickets = ticketsWithSla.Count(t => IsSlaBreached(t));
        var slaCompliancePercentage = ticketsWithSla.Any()
            ? Math.Round((double)(ticketsWithSla.Count - breachedTickets) / ticketsWithSla.Count * 100, 1)
            : 100.0;

        var resolvedTickets = tickets.Where(t => t.ResolvedAt.HasValue).ToList();
        var averageResolutionTime = resolvedTickets.Any()
            ? resolvedTickets.Average(t => (t.ResolvedAt!.Value - t.CreatedAt).TotalMinutes)
            : 0;

        var openTickets = tickets.Count(t => t.Status == TicketStatus.Open || t.Status == TicketStatus.InProgress);

        var topAgent = agentWorkload.OrderByDescending(a => a.Resolved).FirstOrDefault();

        return new TicketAnalyticsDto
        {
            WeeklyVolume = weeklyVolume,
            StatusDistribution = statusDistribution,
            PriorityDistribution = priorityDistribution,
            AgentWorkload = agentWorkload,
            SlaCompliancePercentage = slaCompliancePercentage,
            AverageResolutionTime = FormatDuration(averageResolutionTime),
            Insights = new List<InsightItem>
            {
                new()
                {
                    Label = "Avg. resolution time",
                    Value = FormatDuration(averageResolutionTime),
                    Change = "Based on resolved tickets",
                    ChangeUp = true
                },
                new()
                {
                    Label = "Open tickets",
                    Value = openTickets.ToString(),
                    Change = $"{tickets.Count(t => t.Status == TicketStatus.Open)} unassigned/open",
                    ChangeUp = false
                },
                new()
                {
                    Label = "SLA breaches",
                    Value = breachedTickets.ToString(),
                    Change = $"{slaCompliancePercentage:F0}% compliance",
                    ChangeUp = slaCompliancePercentage >= 80
                },
                new()
                {
                    Label = "Top agent",
                    Value = topAgent?.Name ?? "N/A",
                    Change = $"{topAgent?.Resolved ?? 0} resolved",
                    ChangeUp = true
                }
            }
        };
    }

    private static bool IsSlaBreached(Ticket ticket)
    {
        if (!ticket.SlaDueAt.HasValue)
            return false;

        if (ticket.ResolvedAt.HasValue)
            return ticket.ResolvedAt.Value > ticket.SlaDueAt.Value;

        if (ticket.Status == TicketStatus.Resolved || ticket.Status == TicketStatus.Closed)
            return false;

        return DateTime.UtcNow > ticket.SlaDueAt.Value;
    }

    private static string FormatDuration(double totalMinutes)
    {
        if (totalMinutes <= 0)
            return "0m";

        var timeSpan = TimeSpan.FromMinutes(totalMinutes);
        var days = (int)timeSpan.TotalDays;
        var hours = timeSpan.Hours;
        var minutes = timeSpan.Minutes;

        if (days > 0)
        {
            var parts = new List<string>();
            parts.Add($"{days} day{(days == 1 ? "" : "s")}");
            if (hours > 0) parts.Add($"{hours} hr{(hours == 1 ? "" : "s")}");
            if (minutes > 0) parts.Add($"{minutes} min{(minutes == 1 ? "" : "s")}");
            return string.Join(" ", parts);
        }

        if (hours > 0)
        {
            var parts = new List<string>();
            parts.Add($"{hours} hr{(hours == 1 ? "" : "s")}");
            if (minutes > 0) parts.Add($"{minutes} min{(minutes == 1 ? "" : "s")}");
            return string.Join(" ", parts);
        }

        return $"{minutes} min{(minutes == 1 ? "" : "s")}";
    }
}
