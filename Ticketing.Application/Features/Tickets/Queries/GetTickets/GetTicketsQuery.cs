using MediatR;
using Ticketing.Application.DTOs;
using Ticketing.Domain.Enums;

namespace Ticketing.Application.Features.Tickets.Queries.GetTickets;

public record GetTicketsQuery(
    Guid? DepartmentId,
    Guid? CategoryId,
    TicketStatus? Status,
    TicketPriority? Priority,
    DateTime? FromDate,
    DateTime? ToDate) : IRequest<IReadOnlyList<TicketResponseDto>>;
