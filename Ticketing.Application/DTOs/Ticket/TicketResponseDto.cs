namespace Ticketing.Application.DTOs;

public class TicketResponseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;

    public Guid? AssignedAgentId { get; set; }
    public string? AssignedAgentName { get; set; }
    public Guid CreatedById { get; set; }
    public string CreatedByName { get; set; } = string.Empty;

    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public Guid DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? SlaDueAt { get; set; }
    public bool IsBreached { get; set; }
    public string? OverdueBy { get; set; }
    public bool IsRated { get; set; }
    public string? AttachmentUrl { get; set; }
}
