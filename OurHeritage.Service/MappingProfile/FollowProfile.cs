using AutoMapper;
using OurHeritage.Core.Entities;
using OurHeritage.Service.DTOs.FollowDto;

namespace OurHeritage.Service.MappingProfile
{
    public class FollowProfile : Profile
    {
        public FollowProfile()
        {
            CreateMap<FollowDto, Follow>();

            CreateMap<Follow, GetFollowerDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Follower.Id))
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Follower.UserName));
        }
    }
}
