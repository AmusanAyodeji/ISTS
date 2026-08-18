using Ticketing.Application.DTOs.Users;
using Ticketing.Domain.Entities;

namespace Ticketing.Application.Interfaces.Persistence;

public interface IJobErrorRepository : IGenericRepository<JobError>
{
    Task<List<JobError>> GetErrorsByJobId(Guid JobId, CancellationToken cancellationToken);
}