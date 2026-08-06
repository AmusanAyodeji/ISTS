using Ticketing.Domain.Entities;

namespace Ticketing.Application.Interfaces.Persistence;

public interface INotificationRepository : IGenericRepository<Notification>
{
    Task<IReadOnlyList<Notification>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Notification>> GetUnreadByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Notification?> GetByIdAndUserIdAsync(Guid notificationId, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> HasBreachNotificationForTicketAsync(Guid userId, Guid ticketId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Notification>> GetByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken = default);
}