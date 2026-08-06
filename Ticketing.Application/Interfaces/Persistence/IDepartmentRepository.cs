using Ticketing.Domain.Entities;

namespace Ticketing.Application.Interfaces.Persistence;

public interface IDepartmentRepository : IGenericRepository<Department>
{
    Task<List<Department>> GetAllWithCategoriesAsync();
    Task CreateAsync(Department department);
     Task<bool> DepartmentExistsAsync(
        string name,
        CancellationToken cancellationToken = default);
}
