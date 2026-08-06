using Microsoft.EntityFrameworkCore;
using Ticketing.Application.Interfaces.Persistence;
using Ticketing.Domain.Constants;
using Ticketing.Domain.Entities;
using Ticketing.Infrastructure.Persistence.Context;

namespace Ticketing.Infrastructure.Persistence.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
    }

    public async Task<User?> GetByIdWithRolesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(x => x.Roles)
            .Include(x => x.Department)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<User?> GetByEmailWithRolesAsync(string email, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(x => x.Roles)
            .FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
    }

    public async Task<User?> GetByPasswordResetTokenHashAsync(string passwordResetTokenHash, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(x => x.PasswordResetTokenHash == passwordResetTokenHash, cancellationToken);
    }

    public async Task<bool> DepartmentExistsAsync(Guid departmentId, CancellationToken cancellationToken = default)
    {
        return await Context.Departments.AnyAsync(x => x.Id == departmentId, cancellationToken);
    }

    public async Task<IReadOnlyList<User>> ListWithRolesAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Include(x => x.Roles)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Role>> GetRolesByNamesAsync(IEnumerable<string> roleNames, CancellationToken cancellationToken = default)
    {
        var normalizedNames = roleNames
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return await Context.Roles
            .Where(x => normalizedNames.Contains(x.Name))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<User>> GetManagersByDepartmentAsync(Guid departmentId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Include(x => x.Roles)
            .Where(x => x.DepartmentId == departmentId && x.Roles.Any(r => r.Name == SystemRoles.Manager))
            .ToListAsync(cancellationToken);
    }
}
