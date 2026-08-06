using MediatR;
using Ticketing.Application.DTOs.Notifications;

namespace Ticketing.Application.Features.Notifications.Queries.GetNotifications;

public record GetNotificationsQuery(Guid UserId) : IRequest<IReadOnlyList<NotificationDto>>;