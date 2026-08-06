using Ticketing.Domain.Entities;
using Ticketing.Domain.Enums;

namespace Ticketing.Application.Interfaces.Persistence;

public interface ITicketRepository : IGenericRepository<Ticket>
{
    Task<IReadOnlyList<Ticket>> GetByDepartmentAsync(Guid departmentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Ticket>> GetFilteredAsync(
    Guid? departmentId,
    Guid? categoryId,
    TicketStatus? status,
    TicketPriority? priority,
    DateTime? fromDate,
    DateTime? toDate,
    CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Ticket>> GetActiveTicketsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Ticket>> GetTicketsCreatedByAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Ticket>> GetTicketsAssignedToAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Ticket?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Ticket>> GetUnresolvedTickets(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Ticket>> GetBreachedTicketsAsync(CancellationToken cancellationToken = default);
}
