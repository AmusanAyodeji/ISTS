namespace Ticketing.Application.DTOs.Messages;

public class TicketMessageDto
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public Guid SenderUserId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsInternal { get; set; }
    public string? AttachmentUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}