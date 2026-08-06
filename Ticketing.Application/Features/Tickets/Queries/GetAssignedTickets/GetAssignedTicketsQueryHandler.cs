using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Ticketing.Application.DTOs;
using Ticketing.Application.Interfaces.Persistence;
using Ticketing.Application.Interfaces.Services;

namespace Ticketing.Application.Features.Tickets.Queries.GetAssignedTickets;

public class GetAssignedTicketsQueryHandler : IRequestHandler<GetAssignedTicketsQuery, IReadOnlyList<TicketResponseDto>>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IRatingRepository _ratingRepository;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public GetAssignedTicketsQueryHandler(
        ITicketRepository ticketRepository,
        IRatingRepository ratingRepository,
        IMapper mapper,
        ICurrentUserService currentUserService)
    {
        _ticketRepository = ticketRepository;
        _ratingRepository = ratingRepository;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<TicketResponseDto>> Handle(GetAssignedTicketsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var tickets = await _ticketRepository.GetTicketsAssignedToAsync(userId.Value, cancellationToken);
        var dtos = _mapper.Map<IReadOnlyList<TicketResponseDto>>(tickets);

        var ticketIds = dtos.Select(d => d.Id).ToList();
        var ratedIds = await _ratingRepository.GetRatedTicketIdsByUserAsync(userId.Value, ticketIds, cancellationToken);
        foreach (var dto in dtos)
        {
            dto.IsRated = ratedIds.Contains(dto.Id);
        }

        return dtos;
    }
}
