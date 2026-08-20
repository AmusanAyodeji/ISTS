using MediatR;
using Ticketing.Application.DTOs;
using Ticketing.Application.DTOs.Department;

namespace Ticketing.Application.Features.Departments.Commands.UpdateDepartment;

public record UpdateDepartmentCommand(Guid Id, UpdateDepartmentDto Request)
    : IRequest<DepartmentResponseDto>;
