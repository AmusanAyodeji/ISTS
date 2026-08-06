namespace Ticketing.Application.DTOs;

public class TicketAnalyticsDto
{
    public List<WeeklyTicketPoint> WeeklyVolume { get; set; } = new();
    public List<StatusDistributionSegment> StatusDistribution { get; set; } = new();
    public List<PriorityDistributionSegment> PriorityDistribution { get; set; } = new();
    public List<AgentWorkloadEntry> AgentWorkload { get; set; } = new();
    public double SlaCompliancePercentage { get; set; }
    public string AverageResolutionTime { get; set; } = string.Empty;
    public List<InsightItem> Insights { get; set; } = new();
}

public class WeeklyTicketPoint
{
    public string Label { get; set; } = string.Empty;
    public int Received { get; set; }
    public int Resolved { get; set; }
}

public class StatusDistributionSegment
{
    public string Label { get; set; } = string.Empty;
    public int Value { get; set; }
    public string Color { get; set; } = string.Empty;
}

public class PriorityDistributionSegment
{
    public string Label { get; set; } = string.Empty;
    public int Value { get; set; }
    public string Color { get; set; } = string.Empty;
}

public class AgentWorkloadEntry
{
    public string Name { get; set; } = string.Empty;
    public string Initials { get; set; } = string.Empty;
    public int Open { get; set; }
    public int Resolved { get; set; }
}

public class InsightItem
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Change { get; set; } = string.Empty;
    public bool ChangeUp { get; set; }
}
