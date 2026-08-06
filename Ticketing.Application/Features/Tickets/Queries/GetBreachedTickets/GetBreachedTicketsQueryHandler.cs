using AutoMapper;
using MediatR;
using Ticketing.Application.DTOs;
using Ticketing.Application.Interfaces.Persistence;
using Ticketing.Application.Interfaces.Services;

namespace Ticketing.Application.Features.Tickets.Queries.GetBreachedTickets;

public class GetBreachedTicketsQueryHandler
    : IRequestHandler<GetBreachedTicketsQuery, IReadOnlyList<TicketResponseDto>>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IRatingRepository _ratingRepository;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public GetBreachedTicketsQueryHandler(
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

    public async Task<IReadOnlyList<TicketResponseDto>> Handle(
    GetBreachedTicketsQuery request,
    CancellationToken cancellationToken)
    {
        var tickets = await _ticketRepository.GetBreachedTicketsAsync(cancellationToken);
        var dtos = _mapper.Map<IReadOnlyList<TicketResponseDto>>(tickets);

        var userId = _currentUserService.UserId;
        if (userId.HasValue)
        {
            var ticketIds = dtos.Select(d => d.Id).ToList();
            var ratedIds = await _ratingRepository.GetRatedTicketIdsByUserAsync(userId.Value, ticketIds, cancellationToken);
            foreach (var dto in dtos)
            {
                dto.IsRated = ratedIds.Contains(dto.Id);
            }
        }

        return dtos;
    }
}