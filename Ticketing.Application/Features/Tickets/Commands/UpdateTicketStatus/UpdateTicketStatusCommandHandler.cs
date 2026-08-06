using System;
using System.Threading;
using AutoMapper;
using MediatR;
using Ticketing.Application.DTOs;
using Ticketing.Application.Interfaces.Persistence;
using Ticketing.Domain.Enums;

namespace Ticketing.Application.Features.Tickets.Commands.UpdateTicketStatus;

public class UpdateTicketStatusCommandHandler : IRequestHandler<UpdateTicketStatusCommand, TicketResponseDto>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IMapper _mapper;

    public UpdateTicketStatusCommandHandler(ITicketRepository ticketRepository, IMapper mapper)
    {
        _ticketRepository = ticketRepository;
        _mapper = mapper;
    }

    public async Task<TicketResponseDto> Handle(UpdateTicketStatusCommand request, CancellationToken cancellationToken)
    {
        // Load the ticket with change tracking and without related-entity includes
        // so Update() does not try to attach an already-tracked Department/User graph.
        var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken);
        if (ticket == null)
        {
            throw new KeyNotFoundException("Ticket not found.");
        }

        if (!IsValidTransition(ticket.Status, request.Request.Status))
        {
            throw new InvalidOperationException("Invalid status transition.");
        }

        ticket.Status = request.Request.Status;

        if ((ticket.Status == TicketStatus.Resolved || ticket.Status == TicketStatus.Closed) && !ticket.ResolvedAt.HasValue)
        {
            ticket.ResolvedAt = DateTime.UtcNow;
        }
        else if (ticket.Status != TicketStatus.Resolved && ticket.Status != TicketStatus.Closed)
        {
            ticket.ResolvedAt = null;
        }

        _ticketRepository.Update(ticket);
        await _ticketRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<TicketResponseDto>(ticket);
    }

    private bool IsValidTransition(TicketStatus current, TicketStatus next)
    {
        if (next == current)
            return true;

        if (current == TicketStatus.Closed)
            return false;

        return (int)next > (int)current;
    }
}
