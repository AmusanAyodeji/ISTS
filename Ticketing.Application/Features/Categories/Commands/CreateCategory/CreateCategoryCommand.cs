using MediatR;
using Ticketing.Application.DTOs;

namespace Ticketing.Application.Features.Categories.Commands.CreateCategory;

public record CreateCategoryCommand(CreateCategoryDto Request) : IRequest<CategoryResponseDto>;