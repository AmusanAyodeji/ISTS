using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ticketing.Application.DTOs.SLA;
using Ticketing.Application.Interfaces.Persistence;
using Ticketing.Domain.Entities;
using Ticketing.Infrastructure.Persistence.Context;
using Ticketing.Domain.Enums;

namespace Ticketing.Infrastructure.Persistence.Repositories
{
    public class SLARepository : GenericRepository<SLA>, ISLARepository
    {
        public SLARepository(AppDbContext context) : base(context)
        {
        }
        public async Task<IReadOnlyList<SLA?>> GetSLA(Guid DepartmentId, CancellationToken cancellationToken = default)
        {
            return await DbSet.Include(x => x.Department).Where(x => x.DepartmentId == DepartmentId).ToListAsync(cancellationToken);
        }
        public async Task<SLA?> GetSLAByPriority(Guid DepartmentId, TicketPriority priority, CancellationToken cancellationToken = default)
        {
            return await DbSet.Include(x => x.Department).FirstOrDefaultAsync(x => x.DepartmentId == DepartmentId && x.Priority == priority, cancellationToken);
        }
    }
}