using AutoMapper;
using MediatR;
using Ticketing.Application.DTOs;
using Ticketing.Application.Interfaces.Persistence;
using Ticketing.Domain.Entities;

namespace Ticketing.Application.Features.Categories.Commands.UpdateCategory;

public class UpdateCategoryCommandHandler
    : IRequestHandler<UpdateCategoryCommand, CategoryResponseDto>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;

    public UpdateCategoryCommandHandler(
        ICategoryRepository categoryRepository,
        IMapper mapper)
    {
        _categoryRepository = categoryRepository;
        _mapper = mapper;
    }

    public async Task<CategoryResponseDto> Handle(
        UpdateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(request.Id, cancellationToken);

        if (category is null)
        {
            throw new KeyNotFoundException($"Category '{request.Id}' not found.");
        }

        var name = request.Request.Name.Trim();

        if (!name.Equals(category.Name, StringComparison.OrdinalIgnoreCase))
        {
            var exists = await _categoryRepository.CategoryExistsAsync(
                category.DepartmentId,
                name,
                cancellationToken);

            if (exists)
            {
                throw new InvalidOperationException(
                    $"The issue type '{name}' already exists for this department.");
            }
        }

        category.Name = name;

        _categoryRepository.Update(category);
        await _categoryRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CategoryResponseDto>(category);
    }
}
