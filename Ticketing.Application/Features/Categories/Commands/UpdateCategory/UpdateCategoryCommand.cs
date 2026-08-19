using MediatR;
using Ticketing.Application.DTOs;
using Ticketing.Application.DTOs.Category;

namespace Ticketing.Application.Features.Categories.Commands.UpdateCategory;

public record UpdateCategoryCommand(Guid Id, UpdateCategoryDto Request)
    : IRequest<CategoryResponseDto>;
