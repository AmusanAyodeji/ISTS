using AutoMapper;
using Ticketing.Application.DTOs.Department;
using Ticketing.Domain.Entities;
using Ticketing.Application.DTOs;

namespace Ticketing.Application.Common.Mappings;

public class DepartmentMappingProfile : Profile
{
    public DepartmentMappingProfile()
    {
        CreateMap<CreateDepartmentDto, Department>();

        CreateMap<Department, DepartmentResponseDto>();
    }
}