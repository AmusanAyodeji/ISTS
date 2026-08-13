using System;
using System.Threading;
using AutoMapper;
using MediatR;
using Ticketing.Application.DTOs;
using Ticketing.Application.Interfaces.Persistence;
using Ticketing.Domain.Enums;
using Ticketing.Application.Interfaces.Services;
using Ticketing.Application.DTOs.Notifications;
using Ticketing.Domain.Entities;

namespace Ticketing.Application.Features.Tickets.Commands.EscalateTicket;

public class EscalateTicketCommandHandler : IRequestHandler<EscalateTicketCommand, TicketResponseDto>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IMapper _mapper;
    private readonly INotificationHubService _notificationHubService;
    private readonly IUserRepository _userRepository;
    private readonly INotificationRepository _notificationRepository;

    public EscalateTicketCommandHandler(ITicketRepository ticketRepository, IMapper mapper, INotificationHubService notificationHubService, IUserRepository userRepository, INotificationRepository notificationRepository)
    {
        _ticketRepository = ticketRepository;
        _mapper = mapper;
        _notificationHubService = notificationHubService;
        _userRepository = userRepository;
        _notificationRepository = notificationRepository;
    }

    public async Task<TicketResponseDto> Handle(EscalateTicketCommand request, CancellationToken cancellationToken)
    {
        // Load the ticket with change tracking and without related-entity includes
        // so Update() does not try to attach an already-tracked Department/User graph.
        var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken);
        if (ticket == null)
        {
            throw new KeyNotFoundException("Ticket not found.");
        }

        if (ticket.Status == TicketStatus.Resolved || ticket.Status == TicketStatus.Closed)
        {
            throw new InvalidOperationException("Cannot escalate a ticket that is already resolved or closed.");
        }

        ticket.Status = TicketStatus.Open;
        ticket.AssignedToId = null;
        ticket.ResolvedAt = null;

        _ticketRepository.Update(ticket);
        await _ticketRepository.SaveChangesAsync(cancellationToken);

        var updatedticket = _mapper.Map<TicketResponseDto>(ticket);
        await _notificationHubService.NotifyTicketStatusChangedAsync(updatedticket.Id, updatedticket);
        foreach(var manager in await _userRepository.GetManagersByDepartmentAsync(ticket.DepartmentId, cancellationToken))
        {
            var notification = new Notification
            {
                UserId = manager.Id,
                Title = "Escalated Ticket",
                Message = $"A ticket has been escalated: {ticket.Title}",
                Type = NotificationType.ChatMessage,
                TicketId = ticket.Id
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
            await _notificationHubService.NotifyUserAsync(manager.Id, notificationDto);
        }
        return updatedticket;
    }
}
