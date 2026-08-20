using System.Globalization;
using CsvHelper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ticketing.API.Common.Models;
using Ticketing.Application.Common.Mappings;
using Ticketing.Application.DTOs;
using Ticketing.Application.DTOs.Department;
using Ticketing.Application.Features.Departments.Commands.CreateDepartment;
using Ticketing.Application.Features.Departments.Commands.CreateDepartmentsBulk;
using Ticketing.Application.Features.Departments.Commands.DeleteDepartment;
using Ticketing.Application.Features.Departments.Commands.UpdateDepartment;
using Ticketing.Application.Interfaces.Persistence;
using Ticketing.Domain.Entities;


namespace Ticketing.API.Controllers;

[ApiController]
[Route("api/departments")]
public class DepartmentController : ControllerBase
{
    private readonly IDepartmentRepository _repo;
    private readonly IMediator _mediator;

    public DepartmentController(IDepartmentRepository repo, IMediator mediator)
    {
        _repo = repo;
        _mediator = mediator;
    }

    private static DepartmentResponseDto MapToResponse(Department department)
    {
        return new DepartmentResponseDto
        {
            Id = department.Id,
            Name = department.Name,
            Description = department.Description,
            Categories = department.Categories
                .Select(c => new CategoryResponseDto { Id = c.Id, Name = c.Name })
                .ToList()
        };
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Create(CreateDepartmentDto dto)
    {
        var result = await _mediator.Send(new CreateDepartmentCommand(dto));
        return Ok(ApiResponse<DepartmentResponseDto>.Success(
            result,
            "Department created successfully."));
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll()
    {
        var departments = await _repo.GetAllWithCategoriesAsync();
        var response = departments.Select(MapToResponse).ToList();

        return Ok(ApiResponse<IReadOnlyList<DepartmentResponseDto>>.Success(
            response,
            "Departments retrieved successfully."));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse<DepartmentResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDepartmentDto dto, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new UpdateDepartmentCommand(id, dto), cancellationToken);
        return Ok(ApiResponse<DepartmentResponseDto>.Success(result, "Department updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteDepartmentCommand(id), cancellationToken);
        return Ok(ApiResponse<object>.Success(new { Id = id }, "Department deleted successfully."));
    }

    [HttpPost("upload")]
    [Authorize(Policy = "AdminOnly")]
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
            csv.Context.RegisterClassMap<DepartmentInfoMap>();
            var records = new List<DepartmentInfo>();
            await foreach (var record in csv.GetRecordsAsync<DepartmentInfo>(cancellationToken))
            {
                records.Add(record);
            }

            var result = await _mediator.Send(new CreateDepartmentsBulkCommand(records), cancellationToken);
            return Ok(ApiResponse<BulkImportResult>.Success(result, "Departments imported successfully."));
        }
        catch (HeaderValidationException)
        {
            return BadRequest(
                ApiResponse<object>.Failure(["The uploaded file does not match the required template. Expected columns: Name, Description."]));
        }
    }
}
