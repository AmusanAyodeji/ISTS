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

namespace Ticketing.Application.Features.Tickets.Commands.CloseTicket;

public class CloseTicketCommandHandler : IRequestHandler<CloseTicketCommand, Unit>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly INotificationHubService _notificationHubService;
    private readonly ICurrentUserService _currentUserService;

    public CloseTicketCommandHandler(
        ITicketRepository ticketRepository,
        IUserRepository userRepository,
        IMapper mapper,
        INotificationHubService notificationHubService,
        ICurrentUserService currentUserService)
    {
        _ticketRepository = ticketRepository;
        _userRepository = userRepository;
        _mapper = mapper;
        _notificationHubService = notificationHubService;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(CloseTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken);
        if (ticket == null)
        {
            throw new KeyNotFoundException("Ticket not found.");
        }
        if (ticket.AssignedToId is null)
        {
            throw new InvalidOperationException("Ticket is not assigned to anyone");
        }
        if (ticket.Status == TicketStatus.Closed)
        {
            throw new InvalidOperationException("Ticket is already closed.");
        }
        if(ticket.AssignedToId != _currentUserService.UserId.Value)
        {
            throw new InvalidOperationException("Agent doesnt have permissions to close ticket");
        }

        ticket.Status = TicketStatus.Closed;

        var closedticket = _mapper.Map<TicketResponseDto>(ticket);
        await _notificationHubService.NotifyTicketStatusChangedAsync(closedticket.Id, closedticket);

        _ticketRepository.Update(ticket);
        await _ticketRepository.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
