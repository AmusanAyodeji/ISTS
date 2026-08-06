using Microsoft.EntityFrameworkCore;
using Ticketing.Application.Interfaces.Persistence;
using Ticketing.Domain.Entities;
using Ticketing.Infrastructure.Persistence.Context;

namespace Ticketing.Infrastructure.Persistence.Repositories
{
    public class RatingRepository : GenericRepository<Ratings>, IRatingRepository
    {
        public RatingRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Ratings?> GetByTicketId(Guid TickerId, CancellationToken cancellation = default)
        {
            return await DbSet.FirstOrDefaultAsync(x => x.TicketId == TickerId);
        }

        public async Task<IReadOnlySet<Guid>> GetRatedTicketIdsByUserAsync(Guid userId, IEnumerable<Guid> ticketIds, CancellationToken cancellationToken = default)
        {
            var ids = ticketIds.ToList();
            var rated = await DbSet
                .AsNoTracking()
                .Where(x => x.UserId == userId && ids.Contains(x.TicketId))
                .Select(x => x.TicketId)
                .ToListAsync(cancellationToken);

            return new HashSet<Guid>(rated);
        }

        public async Task<double?> GetAverageRatingForAgentAsync(Guid agentId, CancellationToken cancellationToken = default)
        {
            return await DbSet
                .AsNoTracking()
                .Include(x => x.Ticket)
                .Where(x => x.Ticket != null && x.Ticket.AssignedToId == agentId)
                .AverageAsync(x => (double?)x.Rating, cancellationToken);
        }

        public async Task<double?> GetAverageRatingAsync(CancellationToken cancellationToken = default)
        {
            return await DbSet
                .AsNoTracking()
                .AverageAsync(x => (double?)x.Rating, cancellationToken);
        }

        public async Task<int> GetRatingCountForAgentAsync(Guid agentId, CancellationToken cancellationToken = default)
        {
            return await DbSet
                .AsNoTracking()
                .Include(x => x.Ticket)
                .CountAsync(x => x.Ticket != null && x.Ticket.AssignedToId == agentId, cancellationToken);
        }

        public async Task<int> GetRatingCountAsync(CancellationToken cancellationToken = default)
        {
            return await DbSet
                .AsNoTracking()
                .CountAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Ratings>> ListByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken = default)
        {
            return await DbSet
                .Where(x => x.TicketId == ticketId)
                .ToListAsync(cancellationToken);
        }
    }
}
