using MediatR;
using Ticketing.Application.DTOs.Department;
using Ticketing.Application.Interfaces.Persistence;
using Ticketing.Domain.Entities;

namespace Ticketing.Application.Features.Departments.Commands.CreateDepartmentsBulk;

public class CreateDepartmentsBulkCommandHandler
    : IRequestHandler<CreateDepartmentsBulkCommand, BulkImportResult>
{
    private readonly IDepartmentRepository _departmentRepository;

    public CreateDepartmentsBulkCommandHandler(IDepartmentRepository departmentRepository)
    {
        _departmentRepository = departmentRepository;
    }

    public async Task<BulkImportResult> Handle(
        CreateDepartmentsBulkCommand request,
        CancellationToken cancellationToken)
    {
        var result = new BulkImportResult();
        var processedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var info in request.Departments)
        {
            var name = info.Name?.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                result.Skipped++;
                continue;
            }

            if (!processedNames.Add(name))
            {
                result.Skipped++;
                continue;
            }

            var exists = await _departmentRepository.DepartmentExistsAsync(name, cancellationToken);
            if (exists)
            {
                result.Skipped++;
                continue;
            }

            var department = new Department
            {
                Id = Guid.NewGuid(),
                Name = name,
                Description = info.Description?.Trim() ?? string.Empty
            };

            await _departmentRepository.AddAsync(department, cancellationToken);
            result.Created++;
        }

        if (result.Created > 0)
        {
            await _departmentRepository.SaveChangesAsync(cancellationToken);
        }

        return result;
    }
}
