using Microsoft.EntityFrameworkCore;
using Ticketing.Application.Interfaces.Persistence;
using Ticketing.Domain.Entities;
using Ticketing.Infrastructure.Persistence.Context;

namespace Ticketing.Infrastructure.Persistence.Repositories;

public class FileAttachmentRepository : GenericRepository<FileAttachment>, IFileAttachmentRepository
{
    public FileAttachmentRepository(AppDbContext context) : base(context) { }

    public async Task<FileAttachment?> GetByIdWithUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(f => f.UploadedByUser)
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }
}