using Ticketing.Application.DTOs.SLA;
using Ticketing.Domain.Entities;
using Ticketing.Domain.Enums;

namespace Ticketing.Application.Interfaces.Persistence
{
    public interface ISLARepository : IGenericRepository<SLA>
    {
        Task<IReadOnlyList<SLA?>> GetSLA(Guid DepartmentId, CancellationToken cancellationToken = default);
        Task<SLA?> GetSLAByPriority(Guid DepartmentId, TicketPriority Priority, CancellationToken cancellationToken = default);
    }
}