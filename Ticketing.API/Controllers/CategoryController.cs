using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ticketing.API.Common.Models;
using Ticketing.Application.DTOs;
using Ticketing.Application.Features.Categories.Commands.CreateCategory;
using Ticketing.Application.Features.Categories.Queries.GetAllCategories;
using Ticketing.Application.Features.Categories.Queries.GetCategoriesByDepartment;

namespace Ticketing.API.Controllers;

[Route("api/categories")]
public class CategoryController : BaseApiController
{
    [HttpPost]
    [Authorize(Policy = "ManagerOrAdmin")]
    [ProducesResponseType(typeof(ApiResponse<CategoryResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<CategoryResponseDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateCategoryDto request,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new CreateCategoryCommand(request),
            cancellationToken);

        return StatusCode(
            StatusCodes.Status201Created,
            ApiResponse<CategoryResponseDto>.Success(
                result,
                "Category created successfully."));
    }

    [HttpGet]
    [Authorize(Policy = "ManagerOrAdmin")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<CategoryResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new GetAllCategoriesQuery(),
            cancellationToken);

        return StatusCode(
            StatusCodes.Status200OK,
            ApiResponse<IReadOnlyList<CategoryResponseDto>>.Success(
                result,
                "Categories retrieved successfully."));
    }

    [HttpGet("department/{departmentId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<CategoryResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByDepartment(
        Guid departmentId,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new GetCategoriesByDepartmentQuery(departmentId),
            cancellationToken);

        return StatusCode(
            StatusCodes.Status200OK,
            ApiResponse<IReadOnlyList<CategoryResponseDto>>.Success(
                result,
                "Categories retrieved successfully."));
    }
}