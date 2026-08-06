using MediatR;
using Ticketing.Application.DTOs;

namespace Ticketing.Application.Features.Tickets.Queries.GetTicketById;

public record GetTicketByIdQuery(Guid TicketId) : IRequest<TicketResponseDto>;
