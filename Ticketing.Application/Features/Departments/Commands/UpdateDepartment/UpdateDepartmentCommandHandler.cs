using AutoMapper;
using MediatR;
using Ticketing.Application.DTOs;
using Ticketing.Application.DTOs.Department;
using Ticketing.Application.Interfaces.Persistence;
using Ticketing.Domain.Entities;

namespace Ticketing.Application.Features.Departments.Commands.UpdateDepartment;

public class UpdateDepartmentCommandHandler
    : IRequestHandler<UpdateDepartmentCommand, DepartmentResponseDto>
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IMapper _mapper;

    public UpdateDepartmentCommandHandler(
        IDepartmentRepository departmentRepository,
        IMapper mapper)
    {
        _departmentRepository = departmentRepository;
        _mapper = mapper;
    }

    public async Task<DepartmentResponseDto> Handle(
        UpdateDepartmentCommand request,
        CancellationToken cancellationToken)
    {
        var department = await _departmentRepository.GetByIdWithCategoriesAsync(request.Id, cancellationToken);

        if (department is null)
        {
            throw new KeyNotFoundException($"Department '{request.Id}' not found.");
        }

        var name = request.Request.Name.Trim();

        if (!name.Equals(department.Name, StringComparison.OrdinalIgnoreCase))
        {
            var exists = await _departmentRepository.DepartmentExistsAsync(name, cancellationToken);
            if (exists)
            {
                throw new InvalidOperationException($"Department '{name}' already exists.");
            }
        }

        department.Name = name;
        department.Description = request.Request.Description.Trim();

        _departmentRepository.Update(department);
        await _departmentRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<DepartmentResponseDto>(department);
    }
}
