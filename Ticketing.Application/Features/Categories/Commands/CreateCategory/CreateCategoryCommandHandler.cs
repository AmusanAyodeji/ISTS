using AutoMapper;
using MediatR;
using Ticketing.Application.DTOs;
using Ticketing.Application.Interfaces.Persistence;
using Ticketing.Domain.Entities;

namespace Ticketing.Application.Features.Categories.Commands.CreateCategory;

public class CreateCategoryCommandHandler
    : IRequestHandler<CreateCategoryCommand, CategoryResponseDto>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;

    public CreateCategoryCommandHandler(
        ICategoryRepository categoryRepository,
        IMapper mapper)
    {
        _categoryRepository = categoryRepository;
        _mapper = mapper;
    }

    public async Task<CategoryResponseDto> Handle(
        CreateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var department = await _categoryRepository.GetDepartmentByIdAsync(request.Request.DepartmentId);

        if (department is null)
        {
            throw new InvalidOperationException("Department does not exist.");
        }

        var categoryName = request.Request.Name.Trim();

var categoryExists = await _categoryRepository.CategoryExistsAsync(
    request.Request.DepartmentId,
    categoryName,
    cancellationToken);

if (categoryExists)
{
    throw new InvalidOperationException(
        $"The issue type '{categoryName}' already exists for this department.");
}

var category = _mapper.Map<Category>(request.Request);

category.Id = Guid.NewGuid();
category.Name = categoryName;

await _categoryRepository.AddAsync(category, cancellationToken);
await _categoryRepository.SaveChangesAsync(cancellationToken);

return _mapper.Map<CategoryResponseDto>(category);
    }
}