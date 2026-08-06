using Ticketing.Domain.Entities;

namespace Ticketing.Application.Interfaces.Persistence;

public interface IFileAttachmentRepository : IGenericRepository<FileAttachment>
{
    Task<FileAttachment?> GetByIdWithUserAsync(Guid id, CancellationToken cancellationToken = default);
}