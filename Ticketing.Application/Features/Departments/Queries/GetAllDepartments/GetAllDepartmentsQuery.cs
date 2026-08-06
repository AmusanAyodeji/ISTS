using MediatR;
using Ticketing.Application.DTOs.Department;

namespace Ticketing.Application.Features.Departments.Queries.GetAllDepartments;

public record GetAllDepartmentsQuery
    : IRequest<List<DepartmentResponseDto>>;