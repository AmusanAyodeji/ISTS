using Ticketing.Domain.Common;

namespace Ticketing.Domain.Entities;

public class TicketMessage : BaseEntity
{
    public string Message { get; set; } = string.Empty;
    public Guid TicketId { get; set; }
    public Ticket Ticket { get; set; } = null!;

    public Guid SenderUserId { get; set; }
    public User SenderUser { get; set; } = null!;
    public bool IsInternal { get; set; }
    public string? AttachmentUrl { get; set; }
}
