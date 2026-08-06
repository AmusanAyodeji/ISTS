using Microsoft.EntityFrameworkCore;
using Ticketing.Application.Interfaces.Persistence;
using Ticketing.Domain.Entities;
using Ticketing.Infrastructure.Persistence.Context;

namespace Ticketing.Infrastructure.Persistence.Repositories;

public class TicketMessageRepository : GenericRepository<TicketMessage>, ITicketMessageRepository
{
    public TicketMessageRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<TicketMessage>> GetByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Include(m => m.SenderUser)
            .Where(m => m.TicketId == ticketId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TicketMessage>> GetByTicketIdNoTrackingAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(m => m.TicketId == ticketId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<TicketMessage?> GetByIdWithSenderAsync(Guid messageId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(m => m.SenderUser)
            .FirstOrDefaultAsync(m => m.Id == messageId, cancellationToken);
    }
}