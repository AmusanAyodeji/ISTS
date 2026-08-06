using System.Collections.Generic;
using MediatR;
using Ticketing.Application.DTOs;

namespace Ticketing.Application.Features.Tickets.Queries.GetMyTickets;

public record GetMyTicketsQuery : IRequest<IReadOnlyList<TicketResponseDto>>;
