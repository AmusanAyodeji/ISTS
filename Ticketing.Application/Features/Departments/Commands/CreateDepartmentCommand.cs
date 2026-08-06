using MediatR;
using Ticketing.Application.DTOs;
using Ticketing.Application.DTOs.Department;

namespace Ticketing.Application.Features.Departments.Commands.CreateDepartment;

public record CreateDepartmentCommand(CreateDepartmentDto Request)
    : IRequest<DepartmentResponseDto>;