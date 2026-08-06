namespace Ticketing.Application.DTOs.Messages;

public class SendTicketMessageRequestDto
{
    public string Message { get; set; } = string.Empty;
    public bool IsInternal { get; set; }
    public string? AttachmentUrl { get; set; }
}