using AutoMapper;
using MediatR;
using Ticketing.Application.DTOs;
using Ticketing.Application.Interfaces.Persistence;
using Ticketing.Application.DTOs.Department;

namespace Ticketing.Application.Features.Departments.Queries.GetAllDepartments;

public class GetAllDepartmentsQueryHandler
    : IRequestHandler<GetAllDepartmentsQuery, List<DepartmentResponseDto>>
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IMapper _mapper;

    public GetAllDepartmentsQueryHandler(
        IDepartmentRepository departmentRepository,
        IMapper mapper)
    {
        _departmentRepository = departmentRepository;
        _mapper = mapper;
    }

    public async Task<List<DepartmentResponseDto>> Handle(
        GetAllDepartmentsQuery request,
        CancellationToken cancellationToken)
    {
        var departments = await _departmentRepository.GetAllWithCategoriesAsync();

        return _mapper.Map<List<DepartmentResponseDto>>(departments);
    }
}