using System.Collections.Generic;
using MediatR;
using Ticketing.Application.DTOs;

namespace Ticketing.Application.Features.Tickets.Queries.GetAvailableTickets;

public record GetAvailableTicketsQuery(Guid departmentId) : IRequest<IReadOnlyList<TicketResponseDto>>;
