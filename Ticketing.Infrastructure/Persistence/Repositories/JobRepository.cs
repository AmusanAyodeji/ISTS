using Microsoft.EntityFrameworkCore;
using Ticketing.Application.Interfaces.Persistence;
using Ticketing.Domain.Entities;
using Ticketing.Infrastructure.Persistence.Context;
using Ticketing.Domain.Enums;

namespace Ticketing.Infrastructure.Persistence.Repositories;

public class JobRepository : GenericRepository<Job>, IJobRepository
{
    public JobRepository(AppDbContext context) : base(context) { }

    public async Task<Job> GetStatusByJobIdAsync(Guid JobId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(n => n.JobId == JobId, cancellationToken);
    }
    public async Task UpdateStatusByJobIdAsync(Guid JobId, JobStatus Status, CancellationToken cancellationToken = default)
    {
        var job = await DbSet.FirstOrDefaultAsync(n => n.JobId == JobId, cancellationToken);

        if (job == null)
            throw new Exception($"Job {JobId} was not found.");

        job.Status = Status;
    }
}