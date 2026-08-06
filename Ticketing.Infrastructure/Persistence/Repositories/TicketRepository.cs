using Microsoft.EntityFrameworkCore;
using Ticketing.Application.Interfaces.Persistence;
using Ticketing.Domain.Entities;
using Ticketing.Infrastructure.Persistence.Context;
using Ticketing.Domain.Enums;

namespace Ticketing.Infrastructure.Persistence.Repositories;

public class TicketRepository : GenericRepository<Ticket>, ITicketRepository
{
    public TicketRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Ticket>> GetByDepartmentAsync(Guid departmentId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(x => x.DepartmentId == departmentId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }
    public async Task<IReadOnlyList<Ticket>> GetFilteredAsync(
    Guid? departmentId,
    Guid? categoryId,
    TicketStatus? status,
    TicketPriority? priority,
    DateTime? fromDate,
    DateTime? toDate,
    CancellationToken cancellationToken = default)
{
    var query = DbSet.AsNoTracking()
        .Include(x => x.CreatedBy)
        .Include(x => x.AssignedTo)
        .Include(x => x.Category)
        .Include(x => x.Department)
        .AsQueryable();

    if (departmentId.HasValue)
        query = query.Where(x => x.DepartmentId == departmentId);

    if (categoryId.HasValue)
        query = query.Where(x => x.CategoryId == categoryId);

    if (status.HasValue)
        query = query.Where(x => x.Status == status);

    if (priority.HasValue)
        query = query.Where(x => x.Priority == priority);

    if (fromDate.HasValue)
        query = query.Where(x => x.CreatedAt >= fromDate);

    if (toDate.HasValue)
        query = query.Where(x => x.CreatedAt <= toDate);

    return await query
        .OrderByDescending(x => x.CreatedAt)
        .ToListAsync(cancellationToken);
}

    public async Task<IReadOnlyList<Ticket>> GetActiveTicketsAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking()
            .Where(x => x.Status == TicketStatus.Open || x.Status == TicketStatus.InProgress)
            .Include(x => x.AssignedTo)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

   public async Task<IReadOnlyList<Ticket>> GetBreachedTicketsAsync(
    CancellationToken cancellationToken = default)
{
    var tickets = await DbSet
        .AsNoTracking()
        .Where(x => x.SlaDueAt.HasValue)
        .Include(x => x.CreatedBy)
        .Include(x => x.AssignedTo)
        .Include(x => x.Category)
        .Include(x => x.Department)
        .OrderByDescending(x => x.CreatedAt)
        .ToListAsync(cancellationToken);

    return tickets.Where(IsSlaBreached).ToList();
}

    private static bool IsSlaBreached(Ticket ticket)
    {
        if (!ticket.SlaDueAt.HasValue)
            return false;

        if (ticket.ResolvedAt.HasValue)
            return ticket.ResolvedAt.Value > ticket.SlaDueAt.Value;

        if (ticket.Status == TicketStatus.Resolved || ticket.Status == TicketStatus.Closed)
            return false;

        return DateTime.UtcNow > ticket.SlaDueAt.Value;
    }

    public async Task<IReadOnlyList<Ticket>> GetTicketsCreatedByAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking()
            .Where(x => x.CreatedById == userId)
            .Include(x => x.CreatedBy)
            .Include(x => x.AssignedTo)
            .Include(x => x.Category)
            .Include(x => x.Department)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Ticket>> GetTicketsAssignedToAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking()
            .Where(x => x.AssignedToId == userId)
            .Include(x => x.CreatedBy)
            .Include(x => x.AssignedTo)
            .Include(x => x.Category)
            .Include(x => x.Department)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Ticket?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking()
            .Include(x => x.CreatedBy)
            .Include(x => x.AssignedTo)
            .Include(x => x.Category)
            .Include(x => x.Department)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }
    public async Task<IReadOnlyList<Ticket>> GetUnresolvedTickets(CancellationToken cancellationToken = default)
    {
        return await DbSet
           .Where(x => x.Status == TicketStatus.Open || x.Status == TicketStatus.InProgress)
           .OrderByDescending(x => x.CreatedAt)
           .ToListAsync(cancellationToken);
    }
}
