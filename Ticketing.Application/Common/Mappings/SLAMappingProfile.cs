using AutoMapper;
using Ticketing.Application.DTOs.SLA;
using Ticketing.Domain.Entities;

namespace Ticketing.Application.Common.Mappings;

public class SLAMappingProfile : Profile
{
    public SLAMappingProfile()
    {
        CreateMap<CreateSLARequestDTO, SLA>();
        CreateMap<SLA, CreateSLAResponseDTO>();
        CreateMap<SLA, GetSLAResponseDTO>();
        CreateMap<SLA, UpdateSLAResponseDTO>();
        CreateMap<SLA, SLAResponseItemDTO>();
    }
}