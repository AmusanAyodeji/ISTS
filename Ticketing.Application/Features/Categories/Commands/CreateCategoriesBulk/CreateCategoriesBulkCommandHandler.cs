using MediatR;
using Ticketing.Application.DTOs.Category;
using Ticketing.Application.Interfaces.Persistence;
using Ticketing.Domain.Entities;

namespace Ticketing.Application.Features.Categories.Commands.CreateCategoriesBulk;

public class CreateCategoriesBulkCommandHandler
    : IRequestHandler<CreateCategoriesBulkCommand, BulkImportResult>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IDepartmentRepository _departmentRepository;

    public CreateCategoriesBulkCommandHandler(
        ICategoryRepository categoryRepository,
        IDepartmentRepository departmentRepository)
    {
        _categoryRepository = categoryRepository;
        _departmentRepository = departmentRepository;
    }

    public async Task<BulkImportResult> Handle(
        CreateCategoriesBulkCommand request,
        CancellationToken cancellationToken)
    {
        var result = new BulkImportResult();

        var departments = await _departmentRepository.GetAllWithCategoriesAsync();
        var departmentLookup = departments.ToDictionary(
            d => d.Name.Trim(),
            d => d,
            StringComparer.OrdinalIgnoreCase);

        var processedKeys = new HashSet<(string DepartmentName, string CategoryName)>();

        foreach (var info in request.Categories)
        {
            var departmentName = info.DepartmentName?.Trim();
            var categoryName = info.Name?.Trim();

            if (string.IsNullOrWhiteSpace(departmentName) || string.IsNullOrWhiteSpace(categoryName))
            {
                result.Skipped++;
                continue;
            }

            if (!departmentLookup.TryGetValue(departmentName, out var department))
            {
                result.Skipped++;
                continue;
            }

            var key = (departmentName.ToLowerInvariant(), categoryName.ToLowerInvariant());
            if (!processedKeys.Add(key))
            {
                result.Skipped++;
                continue;
            }

            var exists = await _categoryRepository.CategoryExistsAsync(
                department.Id,
                categoryName,
                cancellationToken);

            if (exists)
            {
                result.Skipped++;
                continue;
            }

            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = categoryName,
                DepartmentId = department.Id
            };

            await _categoryRepository.AddAsync(category, cancellationToken);
            result.Created++;
        }

        if (result.Created > 0)
        {
            await _categoryRepository.SaveChangesAsync(cancellationToken);
        }

        return result;
    }
}
