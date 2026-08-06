using MediatR;
using Ticketing.Application.DTOs;

namespace Ticketing.Application.Features.Categories.Queries.GetCategoriesByDepartment;

public record GetCategoriesByDepartmentQuery(Guid DepartmentId)
    : IRequest<IReadOnlyList<CategoryResponseDto>>;