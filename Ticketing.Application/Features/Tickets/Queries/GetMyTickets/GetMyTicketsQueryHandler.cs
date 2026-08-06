using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Ticketing.Application.DTOs;
using Ticketing.Application.Interfaces.Persistence;
using Ticketing.Application.Interfaces.Services;

namespace Ticketing.Application.Features.Tickets.Queries.GetMyTickets;

public class GetMyTicketsQueryHandler : IRequestHandler<GetMyTicketsQuery, IReadOnlyList<TicketResponseDto>>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IRatingRepository _ratingRepository;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public GetMyTicketsQueryHandler(
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

    public async Task<IReadOnlyList<TicketResponseDto>> Handle(GetMyTicketsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var tickets = await _ticketRepository.GetTicketsCreatedByAsync(userId.Value, cancellationToken);
        var ticketIds = tickets.Select(t => t.Id).ToList();
        var ratedTicketIds = await _ratingRepository.GetRatedTicketIdsByUserAsync(userId.Value, ticketIds, cancellationToken);

        var dtos = _mapper.Map<IReadOnlyList<TicketResponseDto>>(tickets);
        foreach (var dto in dtos)
        {
            dto.IsRated = ratedTicketIds.Contains(dto.Id);
        }

        return dtos;
    }
}
