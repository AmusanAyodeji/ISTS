using Ticketing.Domain.Common;
using Ticketing.Domain.Enums;

namespace Ticketing.Domain.Entities;

public class Ticket : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;
    public TicketStatus Status { get; set; } = TicketStatus.Open;

    public Guid DepartmentId { get; set; }
    public Department? Department { get; set; }

    public Guid CategoryId { get; set; }
    public Category? Category { get; set; }

    public Guid CreatedById { get; set; }
    public User CreatedBy { get; set; } = null!;

    public Guid? AssignedToId { get; set; }
    public User? AssignedTo { get; set; }

    public DateTime? SlaDueAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? AttachmentUrl { get; set; }
    public bool SLABreached { get; set; } = false;

    public ICollection<TicketMessage> Messages { get; set; } = new List<TicketMessage>();
}