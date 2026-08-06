using AutoMapper;
using MediatR;
using Ticketing.Application.DTOs;
using Ticketing.Application.Interfaces.Persistence;

namespace Ticketing.Application.Features.Categories.Queries.GetCategoriesByDepartment;

public class GetCategoriesByDepartmentQueryHandler
    : IRequestHandler<GetCategoriesByDepartmentQuery, IReadOnlyList<CategoryResponseDto>>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;

    public GetCategoriesByDepartmentQueryHandler(
        ICategoryRepository categoryRepository,
        IMapper mapper)
    {
        _categoryRepository = categoryRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<CategoryResponseDto>> Handle(
        GetCategoriesByDepartmentQuery request,
        CancellationToken cancellationToken)
    {
        var categories = await _categoryRepository
            .GetByDepartmentIdAsync(request.DepartmentId);

        return _mapper.Map<IReadOnlyList<CategoryResponseDto>>(categories);
    }
}