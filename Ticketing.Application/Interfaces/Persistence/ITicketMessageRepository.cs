using Ticketing.Domain.Entities;

namespace Ticketing.Application.Interfaces.Persistence;

public interface ITicketMessageRepository : IGenericRepository<TicketMessage>
{
    Task<IReadOnlyList<TicketMessage>> GetByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TicketMessage>> GetByTicketIdNoTrackingAsync(Guid ticketId, CancellationToken cancellationToken = default);
    Task<TicketMessage?> GetByIdWithSenderAsync(Guid messageId, CancellationToken cancellationToken = default);
}