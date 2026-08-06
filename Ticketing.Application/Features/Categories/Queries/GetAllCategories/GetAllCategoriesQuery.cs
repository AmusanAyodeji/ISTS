using MediatR;
using Ticketing.Application.DTOs;

namespace Ticketing.Application.Features.Categories.Queries.GetAllCategories;

public record GetAllCategoriesQuery()
    : IRequest<IReadOnlyList<CategoryResponseDto>>;