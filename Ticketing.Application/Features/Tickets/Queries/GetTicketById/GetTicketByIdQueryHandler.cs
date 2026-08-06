using System.Threading;
using AutoMapper;
using MediatR;
using Ticketing.Application.DTOs;
using Ticketing.Application.Interfaces.Persistence;
using Ticketing.Application.Interfaces.Services;

namespace Ticketing.Application.Features.Tickets.Queries.GetTicketById;

public class GetTicketByIdQueryHandler : IRequestHandler<GetTicketByIdQuery, TicketResponseDto>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IRatingRepository _ratingRepository;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public GetTicketByIdQueryHandler(
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

    public async Task<TicketResponseDto> Handle(GetTicketByIdQuery request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken);
        if (ticket == null)
        {
            throw new KeyNotFoundException("Ticket not found.");
        }

        var dto = _mapper.Map<TicketResponseDto>(ticket);

        var userId = _currentUserService.UserId;
        if (userId.HasValue)
        {
            var ratedIds = await _ratingRepository.GetRatedTicketIdsByUserAsync(
                userId.Value, [dto.Id], cancellationToken);
            dto.IsRated = ratedIds.Contains(dto.Id);
        }

        return dto;
    }
}
