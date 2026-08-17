using Ticketing.Domain.Entities;
using Ticketing.Domain.Enums;

namespace Ticketing.Application.Interfaces.Persistence;

public interface IJobRepository : IGenericRepository<Job>
{
    Task<Job> GetStatusByJobIdAsync(Guid JobId, CancellationToken cancellationToken = default);
    Task UpdateStatusByJobIdAsync(Guid JobId, JobStatus Status, CancellationToken cancellationToken = default);
}