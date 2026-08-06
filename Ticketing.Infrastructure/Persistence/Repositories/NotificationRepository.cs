using Microsoft.EntityFrameworkCore;
using Ticketing.Application.Interfaces.Persistence;
using Ticketing.Domain.Entities;
using Ticketing.Infrastructure.Persistence.Context;

namespace Ticketing.Infrastructure.Persistence.Repositories;

public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
{
    public NotificationRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Notification>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Notification>> GetUnreadByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(n => n.UserId == userId && !n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Notification?> GetByIdAndUserIdAsync(Guid notificationId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId, cancellationToken);
    }

    public async Task<bool> HasBreachNotificationForTicketAsync(Guid userId, Guid ticketId, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(n =>
            n.UserId == userId &&
            n.TicketId == ticketId &&
            n.Title == "SLA Breach Alert",
            cancellationToken);
    }

    public async Task<IReadOnlyList<Notification>> GetByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(n => n.TicketId == ticketId)
            .ToListAsync(cancellationToken);
    }
}