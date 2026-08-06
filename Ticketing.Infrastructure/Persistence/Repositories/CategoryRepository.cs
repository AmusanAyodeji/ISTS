using System.Linq;
using Microsoft.EntityFrameworkCore;
using Ticketing.Domain.Entities;
using Ticketing.Application.Interfaces.Persistence;
using Ticketing.Infrastructure.Persistence.Context;


namespace Ticketing.Infrastructure.Persistence.Repositories;
public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
{
    private readonly AppDbContext _context;

    public CategoryRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<List<Category>> GetAllWithDepartmentAsync()
    {
        return await _context.Categories
            .Include(c => c.Department)
            .ToListAsync();
    }

    public async Task CreateAsync(Category category)
    {
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();
    }

    public async Task<Department?> GetDepartmentByIdAsync(Guid departmentId)
    {
        return await _context.Departments
            .FirstOrDefaultAsync(d => d.Id == departmentId);
    }

    public async Task<List<Category>> GetByDepartmentIdAsync(Guid departmentId)
{
    return await _context.Categories
        .Where(c => c.DepartmentId == departmentId)
        .ToListAsync();
}

public async Task<bool> CategoryBelongsToDepartmentAsync(
    Guid categoryId,
    Guid departmentId,
    CancellationToken cancellationToken = default)
{
    return await _context.Categories.AnyAsync(c =>
        c.Id == categoryId &&
        c.DepartmentId == departmentId,
        cancellationToken);
}

public async Task<bool> CategoryExistsAsync(
    Guid departmentId,
    string name,
    CancellationToken cancellationToken = default)
{
    var normalizedName = name.Trim().ToLower();

    return await _context.Categories.AnyAsync(c =>
        c.DepartmentId == departmentId &&
        c.Name.Trim().ToLower() == normalizedName,
        cancellationToken);
}
}