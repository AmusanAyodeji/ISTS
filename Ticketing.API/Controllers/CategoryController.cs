using System.Globalization;
using CsvHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ticketing.API.Common.Models;
using Ticketing.Application.Common.Mappings;
using Ticketing.Application.DTOs;
using Ticketing.Application.DTOs.Category;
using Ticketing.Application.Features.Categories.Commands.CreateCategoriesBulk;
using Ticketing.Application.Features.Categories.Commands.CreateCategory;
using Ticketing.Application.Features.Categories.Commands.DeleteCategory;
using Ticketing.Application.Features.Categories.Commands.UpdateCategory;
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

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse<CategoryResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateCategoryDto request,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new UpdateCategoryCommand(id, request),
            cancellationToken);

        return Ok(ApiResponse<CategoryResponseDto>.Success(result, "Category updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await Mediator.Send(new DeleteCategoryCommand(id), cancellationToken);
        return Ok(ApiResponse<object>.Success(new { Id = id }, "Category deleted successfully."));
    }

    [HttpPost("upload")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse<BulkImportResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Upload(IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || Path.GetExtension(file.FileName).ToLowerInvariant() != ".csv")
        {
            return BadRequest(ApiResponse<object>.Failure(["Only CSV files are supported."]));
        }

        using var reader = new StreamReader(file.OpenReadStream());
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        try
        {
            csv.Context.RegisterClassMap<CategoryInfoMap>();
            var records = new List<CategoryInfo>();
            await foreach (var record in csv.GetRecordsAsync<CategoryInfo>(cancellationToken))
            {
                records.Add(record);
            }

            var result = await Mediator.Send(new CreateCategoriesBulkCommand(records), cancellationToken);
            return Ok(ApiResponse<BulkImportResult>.Success(result, "Categories imported successfully."));
        }
        catch (HeaderValidationException)
        {
            return BadRequest(
                ApiResponse<object>.Failure(["The uploaded file does not match the required template. Expected columns: DepartmentName, Name."]));
        }
    }
}
