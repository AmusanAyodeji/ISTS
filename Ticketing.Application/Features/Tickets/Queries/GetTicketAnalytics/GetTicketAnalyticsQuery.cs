using MediatR;
using Ticketing.Application.DTOs;

namespace Ticketing.Application.Features.Tickets.Queries.GetTicketAnalytics;

public record GetTicketAnalyticsQuery(DateTime? FromDate = null, DateTime? ToDate = null) : IRequest<TicketAnalyticsDto>;
