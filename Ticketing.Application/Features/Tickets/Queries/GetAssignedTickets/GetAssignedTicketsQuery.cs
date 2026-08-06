using System.Collections.Generic;
using MediatR;
using Ticketing.Application.DTOs;

namespace Ticketing.Application.Features.Tickets.Queries.GetAssignedTickets;

public record GetAssignedTicketsQuery : IRequest<IReadOnlyList<TicketResponseDto>>;
