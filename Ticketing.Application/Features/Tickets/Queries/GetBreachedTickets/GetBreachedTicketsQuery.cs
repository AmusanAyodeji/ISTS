using MediatR;
using Ticketing.Application.DTOs;

namespace Ticketing.Application.Features.Tickets.Queries.GetBreachedTickets;

public record GetBreachedTicketsQuery()
    : IRequest<IReadOnlyList<TicketResponseDto>>;