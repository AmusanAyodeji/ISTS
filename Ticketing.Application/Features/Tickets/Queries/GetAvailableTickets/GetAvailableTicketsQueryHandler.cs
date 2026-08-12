using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Ticketing.Application.DTOs;
using Ticketing.Application.Interfaces.Persistence;
using Ticketing.Application.Interfaces.Services;
using Ticketing.Domain.Enums;

namespace Ticketing.Application.Features.Tickets.Queries.GetAvailableTickets;

public class GetAvailableTicketsQueryHandler : IRequestHandler<GetAvailableTicketsQuery, IReadOnlyList<TicketResponseDto>>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IRatingRepository _ratingRepository;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public GetAvailableTicketsQueryHandler(
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

    public async Task<IReadOnlyList<TicketResponseDto>> Handle(GetAvailableTicketsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }
        var tickets = await _ticketRepository.GetFilteredAsync(
             departmentId: request.departmentId,
             categoryId: null,
             status: null,
             priority: null,
             fromDate: null,
             toDate: null,
             cancellationToken: cancellationToken);
        var availabletickets = tickets.Where(d => d.Status == TicketStatus.Open || d.AssignedToId == userId);
        var dtos = _mapper.Map<IReadOnlyList<TicketResponseDto>>(availabletickets);
        var ticketIds = dtos.Select(d => d.Id).ToList();
        var ratedIds = await _ratingRepository.GetRatedTicketIdsByUserAsync(userId.Value, ticketIds, cancellationToken);
        foreach (var dto in dtos)
        {
            dto.IsRated = ratedIds.Contains(dto.Id);
        }

        return dtos;
    }
}
