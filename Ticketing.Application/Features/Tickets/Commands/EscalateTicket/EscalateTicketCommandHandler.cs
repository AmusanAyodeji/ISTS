using System;
using System.Threading;
using AutoMapper;
using MediatR;
using Ticketing.Application.DTOs;
using Ticketing.Application.Interfaces.Persistence;
using Ticketing.Domain.Enums;

namespace Ticketing.Application.Features.Tickets.Commands.EscalateTicket;

public class EscalateTicketCommandHandler : IRequestHandler<EscalateTicketCommand, TicketResponseDto>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IMapper _mapper;

    public EscalateTicketCommandHandler(ITicketRepository ticketRepository, IMapper mapper)
    {
        _ticketRepository = ticketRepository;
        _mapper = mapper;
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

        return _mapper.Map<TicketResponseDto>(ticket);
    }
}
