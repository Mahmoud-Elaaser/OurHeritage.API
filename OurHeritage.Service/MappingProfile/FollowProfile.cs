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
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.FollowerId))
            .ForMember(dest => dest.UserProfilePicture, opt => opt.MapFrom(src => src.Following.ProfilePicture))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => $"{src.Follower.FirstName} {src.Follower.LastName}"));



            CreateMap<Follow, GetFollowingDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.FollowingId))
            .ForMember(dest => dest.UserProfilePicture, opt => opt.MapFrom(src => src.Following.ProfilePicture))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => $"{src.Following.FirstName} {src.Following.LastName}"));

        }
    }
}