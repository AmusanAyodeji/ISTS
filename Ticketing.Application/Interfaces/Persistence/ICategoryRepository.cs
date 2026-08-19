using Ticketing.Domain.Entities;

namespace Ticketing.Application.Interfaces.Persistence;
public interface ICategoryRepository : IGenericRepository<Category>
{
    Task<List<Category>> GetAllWithDepartmentAsync();
    Task CreateAsync(Category category);
    Task<Department?> GetDepartmentByIdAsync(Guid departmentId);
    Task<List<Category>> GetByDepartmentIdAsync(Guid departmentId);
    Task<bool> CategoryBelongsToDepartmentAsync(
    Guid categoryId,
    Guid departmentId,
    CancellationToken cancellationToken = default);
    Task<bool> CategoryExistsAsync(
    Guid departmentId,
    string name,
    CancellationToken cancellationToken = default);
    Task<bool> HasTicketsAsync(
    Guid categoryId,
    CancellationToken cancellationToken = default);
}
