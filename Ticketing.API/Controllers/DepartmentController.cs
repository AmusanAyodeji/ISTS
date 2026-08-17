using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ticketing.API.Common.Models;
using Ticketing.Application.DTOs;
using Ticketing.Application.DTOs.Department;
using Ticketing.Application.Interfaces.Persistence;
using Ticketing.Domain.Entities;


namespace Ticketing.API.Controllers;

[ApiController]
[Route("api/departments")]
[Authorize(Policy = "AdminOnly")]
public class DepartmentController : ControllerBase
{
    private readonly IDepartmentRepository _repo;

    public DepartmentController(IDepartmentRepository repo)
    {
        _repo = repo;
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
    public async Task<IActionResult> Create(CreateDepartmentDto dto)
    {
        var department = new Department
        {
            Id = Guid.NewGuid(),
            Name = dto.Name
        };

        await _repo.CreateAsync(department);

        return Ok(ApiResponse<DepartmentResponseDto>.Success(
            MapToResponse(department),
            "Department created successfully."));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var departments = await _repo.GetAllWithCategoriesAsync();
        var response = departments.Select(MapToResponse).ToList();

        return Ok(ApiResponse<IReadOnlyList<DepartmentResponseDto>>.Success(
            response,
            "Departments retrieved successfully."));
    }
}