using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ticketing.Application.DTOs.Rating;
using Ticketing.Domain.Entities;

namespace Ticketing.Application.Interfaces.Persistence
{
    public interface IRatingRepository : IGenericRepository<Ratings>
    {
        Task<Ratings?> GetByTicketId(Guid TickerId, CancellationToken cancellation = default);
        Task<IReadOnlySet<Guid>> GetRatedTicketIdsByUserAsync(Guid userId, IEnumerable<Guid> ticketIds, CancellationToken cancellationToken = default);
        Task<double?> GetAverageRatingForAgentAsync(Guid agentId, CancellationToken cancellationToken = default);
        Task<double?> GetAverageRatingAsync(CancellationToken cancellationToken = default);
        Task<int> GetRatingCountForAgentAsync(Guid agentId, CancellationToken cancellationToken = default);
        Task<int> GetRatingCountAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Ratings>> ListByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken = default);
    }
}
