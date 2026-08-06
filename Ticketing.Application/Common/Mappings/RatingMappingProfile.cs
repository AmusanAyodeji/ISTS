using AutoMapper;
using Ticketing.Application.DTOs.Rating;
using Ticketing.Domain.Entities;

namespace Ticketing.Application.Common.Mappings
{
    public class RatingMappingProfile : Profile
    {
        public RatingMappingProfile()
        {
            CreateMap<RatingCreationDTO, Ratings>();
        }
    }
}