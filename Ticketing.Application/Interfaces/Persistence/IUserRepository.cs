using Ticketing.Domain.Entities;

namespace Ticketing.Application.Interfaces.Persistence;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByIdWithRolesAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailWithRolesAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByPasswordResetTokenHashAsync(string passwordResetTokenHash, CancellationToken cancellationToken = default);
    Task<bool> DepartmentExistsAsync(Guid departmentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<User>> ListWithRolesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Role>> GetRolesByNamesAsync(IEnumerable<string> roleNames, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<User>> GetManagersByDepartmentAsync(Guid departmentId, CancellationToken cancellationToken = default);
}
