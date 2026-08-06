using Microsoft.EntityFrameworkCore;
using Ticketing.Application.Interfaces.Persistence;
using Ticketing.Domain.Entities;
using Ticketing.Infrastructure.Persistence.Context;

namespace Ticketing.Infrastructure.Persistence.Repositories;
public class DepartmentRepository : GenericRepository<Department>, IDepartmentRepository
{
 private readonly AppDbContext _context;

 public DepartmentRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }
    public async Task<List<Department>> GetAllWithCategoriesAsync()
    {
        return await _context.Departments
        .Include(d => d.Categories)
        .ToListAsync();
    }

    public async Task CreateAsync (Department department)
    {
        await _context.Departments.AddAsync(department);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DepartmentExistsAsync(
    string name,
    CancellationToken cancellationToken = default)
{
    var normalizedName = name.Trim().ToLower();

    return await _context.Departments.AnyAsync(d =>
        d.Name.Trim().ToLower() == normalizedName,
        cancellationToken);
}
}