using System;
using System.Threading;
using AutoMapper;
using MediatR;
using Ticketing.Application.DTOs;
using Ticketing.Application.Interfaces.Persistence;
using Ticketing.Domain.Constants;
using Ticketing.Application.Interfaces.Services;
using Ticketing.Domain.Enums;
using Ticketing.Application.Features.Tickets.Queries.GetTicketById;

namespace Ticketing.Application.Features.Tickets.Commands.ReassignTicket;

public class ReassignTicketCommandHandler : IRequestHandler<ReassignTicketCommand, TicketResponseDto>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IUserRepository _userRepository;
    private readonly ITicketMessageRepository _messageRepository;
    private readonly IMapper _mapper;
    private readonly INotificationHubService _notificationHubService;

    public ReassignTicketCommandHandler(
        ITicketRepository ticketRepository,
        IUserRepository userRepository,
        IMapper mapper,
        INotificationHubService notificationHubService,
        ITicketMessageRepository messageRepository)
    {
        _ticketRepository = ticketRepository;
        _userRepository = userRepository;
        _mapper = mapper;
        _notificationHubService = notificationHubService;
        _messageRepository = messageRepository;
    }

    public async Task<TicketResponseDto> Handle(ReassignTicketCommand request, CancellationToken cancellationToken)
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
            throw new InvalidOperationException("Cannot assign a ticket that is resolved or closed.");
        }

        var agentId = request.Request.AgentId;
        var agent = await _userRepository.GetByIdWithRolesAsync(agentId, cancellationToken);
        if (agent == null)
        {
            throw new KeyNotFoundException("Agent not found.");
        }

        if (!agent.Roles.Any(r => r.Name == SystemRoles.Agent || r.Name == SystemRoles.Manager || r.Name == SystemRoles.Admin))
        {
            throw new InvalidOperationException("Selected user is not authorized to handle tickets.");
        }
        var oldAgent = ticket.AssignedToId;
        ticket.AssignedToId = agentId;
        ticket.Status = TicketStatus.InProgress;

        var updatedticket = _mapper.Map<TicketResponseDto>(ticket);
        await _notificationHubService.NotifyTicketStatusChangedAsync(updatedticket.Id, updatedticket);

        _ticketRepository.Update(ticket);
        await _ticketRepository.SaveChangesAsync(cancellationToken);

        var messages = await _messageRepository.GetByTicketIdAsync(request.TicketId, cancellationToken);
        foreach(var message in messages)
        {
            if(message.SenderUserId == oldAgent)
            {
                message.SenderUserId = agentId;
                message.SenderUser = agent;
                _messageRepository.Update(message);
            }
        }
        await _messageRepository.SaveChangesAsync(cancellationToken);
        return updatedticket;
    }
}
