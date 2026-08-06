using AutoMapper;
using MediatR;
using Ticketing.Application.DTOs;
using Ticketing.Application.Interfaces.Persistence;
using Ticketing.Domain.Entities;
using Ticketing.Application.DTOs.Department;

namespace Ticketing.Application.Features.Departments.Commands.CreateDepartment;

public class CreateDepartmentCommandHandler
    : IRequestHandler<CreateDepartmentCommand, DepartmentResponseDto>
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IMapper _mapper;

    public CreateDepartmentCommandHandler(
        IDepartmentRepository departmentRepository,
        IMapper mapper)
    {
        _departmentRepository = departmentRepository;
        _mapper = mapper;
    }

    public async Task<DepartmentResponseDto> Handle(
        CreateDepartmentCommand request,
        CancellationToken cancellationToken)
    {
        var departmentName = request.Request.Name.Trim();

        var departmentExists = await _departmentRepository.DepartmentExistsAsync(
            departmentName,
            cancellationToken);

        if (departmentExists)
        {
            throw new InvalidOperationException(
                $"Department '{departmentName}' already exists.");
        }

        var department = _mapper.Map<Department>(request.Request);

        department.Id = Guid.NewGuid();
        department.Name = departmentName;

        await _departmentRepository.AddAsync(department, cancellationToken);
        await _departmentRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<DepartmentResponseDto>(department);
    }
}