using System;
using System.Threading;
using AutoMapper;
using MediatR;
using Ticketing.Application.DTOs;
using Ticketing.Application.Interfaces.Persistence;
using Ticketing.Domain.Constants;
using Ticketing.Domain.Enums;

namespace Ticketing.Application.Features.Tickets.Commands.AssignTicket;

public class AssignTicketCommandHandler : IRequestHandler<AssignTicketCommand, TicketResponseDto>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public AssignTicketCommandHandler(
        ITicketRepository ticketRepository,
        IUserRepository userRepository,
        IMapper mapper)
    {
        _ticketRepository = ticketRepository;
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<TicketResponseDto> Handle(AssignTicketCommand request, CancellationToken cancellationToken)
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

        ticket.AssignedToId = agentId;
        ticket.Status = TicketStatus.InProgress;

        _ticketRepository.Update(ticket);
        await _ticketRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<TicketResponseDto>(ticket);
    }
}
