using AutoMapper;
using Ticketing.Application.DTOs.Auth;
using Ticketing.Application.DTOs.Users;
using Ticketing.Domain.Entities;

namespace Ticketing.Application.Common.Mappings;

public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<CreateUserRequestDto, User>()
            .ForMember(dest => dest.Roles, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore());

        CreateMap<RegisterRequestDto, User>()
            .ForMember(dest => dest.Roles, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore());

        CreateMap<User, CreateUserResponseDto>()
            .ForMember(dest => dest.FullName,
                opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
            .ForMember(dest => dest.Roles,
                opt => opt.MapFrom(src => src.Roles.Select(x => x.Name).ToList()));
    }
}
