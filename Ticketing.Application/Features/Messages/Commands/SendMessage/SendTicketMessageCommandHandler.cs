using MediatR;
using Microsoft.Extensions.Logging;
using Ticketing.Application.DTOs.Messages;
using Ticketing.Application.DTOs.Notifications;
using Ticketing.Application.Interfaces.Persistence;
using Ticketing.Application.Interfaces.Services;
using Ticketing.Domain.Entities;
using Ticketing.Domain.Enums;

namespace Ticketing.Application.Features.Messages.Commands.SendMessage;

public class SendTicketMessageCommandHandler : IRequestHandler<SendTicketMessageCommand, TicketMessageDto>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly ITicketMessageRepository _messageRepository;
    private readonly IUserRepository _userRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly INotificationHubService _notificationHubService;
    private readonly IEmailService _emailService;
    private readonly ILogger<SendTicketMessageCommandHandler> _logger;

    public SendTicketMessageCommandHandler(
        ITicketRepository ticketRepository,
        ITicketMessageRepository messageRepository,
        IUserRepository userRepository,
        INotificationRepository notificationRepository,
        INotificationHubService notificationHubService,
        IEmailService emailService,
        ILogger<SendTicketMessageCommandHandler> logger)
    {
        _ticketRepository = ticketRepository;
        _messageRepository = messageRepository;
        _userRepository = userRepository;
        _notificationRepository = notificationRepository;
        _notificationHubService = notificationHubService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<TicketMessageDto> Handle(SendTicketMessageCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken);
        if (ticket is null)
            throw new KeyNotFoundException($"Ticket with ID '{request.TicketId}' not found.");

        var message = new TicketMessage
        {
            TicketId = request.TicketId,
            SenderUserId = request.SenderUserId,
            Message = request.Message,
            IsInternal = request.IsInternal,
            AttachmentUrl = request.AttachmentUrl
        };

        await _messageRepository.AddAsync(message, cancellationToken);
        await _messageRepository.SaveChangesAsync(cancellationToken);

        var savedMessage = await _messageRepository.GetByIdWithSenderAsync(message.Id, cancellationToken);
        if (savedMessage is null)
            throw new InvalidOperationException("Failed to retrieve saved message.");

        var senderName = $"{savedMessage.SenderUser.FirstName} {savedMessage.SenderUser.LastName}";

        var dto = new TicketMessageDto
        {
            Id = savedMessage.Id,
            TicketId = savedMessage.TicketId,
            SenderUserId = savedMessage.SenderUserId,
            SenderName = senderName,
            Message = savedMessage.Message,
            IsInternal = savedMessage.IsInternal,
            AttachmentUrl = savedMessage.AttachmentUrl,
            CreatedAt = savedMessage.CreatedAt
        };

        // Broadcast message via SignalR
        await _notificationHubService.SendMessageToTicketGroupAsync(request.TicketId, dto);

        // Create in-app notification and send email to the other party
        var recipientId = request.SenderUserId == ticket.CreatedById
            ? ticket.AssignedToId
            : ticket.CreatedById;

        if (recipientId.HasValue && recipientId.Value != request.SenderUserId)
        {
            var notification = new Notification
            {
                UserId = recipientId.Value,
                Title = "New Message",
                Message = $"You have a new message on ticket: {ticket.Title}",
                Type = NotificationType.ChatMessage,
                TicketId = request.TicketId
            };

            await _notificationRepository.AddAsync(notification, cancellationToken);
            await _notificationRepository.SaveChangesAsync(cancellationToken);

            var notificationDto = new NotificationDto
            {
                Id = notification.Id,
                Title = notification.Title,
                Message = notification.Message,
                Type = notification.Type,
                IsRead = notification.IsRead,
                ReadAt = notification.ReadAt,
                TicketId = notification.TicketId,
                CreatedAt = notification.CreatedAt
            };

            // Send SignalR notification
            await _notificationHubService.NotifyUserAsync(recipientId.Value, notificationDto);

            // Send email notification (non-blocking)
            var recipient = await _userRepository.GetByIdWithRolesAsync(recipientId.Value, cancellationToken);

            if (recipient is not null && !string.IsNullOrWhiteSpace(recipient.Email))
            {
                try
                {
                    await _emailService.SendAsync(
                        recipient.Email,
                        $"New message on ticket: {ticket.Title}",
                        $"<p>You have a new message on ticket <strong>{ticket.Title}</strong>:</p><p><em>{senderName}</em>: {request.Message}</p>",
                        cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to send notification email to {Email} for ticket {TicketId}.",
                        recipient.Email,
                        ticket.Id);
                }
            }
        }

        return dto;
    }
}