using Microsoft.EntityFrameworkCore;
using Ticketing.Application.Interfaces.Persistence;
using Ticketing.Domain.Entities;
using Ticketing.Infrastructure.Persistence.Context;

namespace Ticketing.Infrastructure.Persistence.Repositories;

public class JobErrorRepository : GenericRepository<JobError>, IJobErrorRepository
{
    public JobErrorRepository(AppDbContext context) : base(context) {
    }

    public async Task<List<JobError>> GetErrorsByJobId(Guid JobId, CancellationToken cancellationToken)
    {
        return await DbSet.Where(j => j.JobId == JobId).ToListAsync(cancellationToken);
    }
}