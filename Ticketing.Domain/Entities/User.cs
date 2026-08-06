using Ticketing.Domain.Common;

namespace Ticketing.Domain.Entities;

public class User : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public string? PasswordResetTokenHash { get; set; }
    public DateTime? PasswordResetTokenExpiresAt { get; set; }

    public Guid? DepartmentId { get; set; }
    public Department? Department { get; set; }

    public ICollection<Role> Roles { get; set; } = new List<Role>();
    public ICollection<Ticket> CreatedTickets { get; set; } = new List<Ticket>();
    public ICollection<Ticket> AssignedTickets { get; set; } = new List<Ticket>();
    public ICollection<TicketMessage> TicketMessages { get; set; } = new List<TicketMessage>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
