using AutoMapper;
using OurHeritage.Core.Entities;
using OurHeritage.Service.DTOs.LikeDto;

namespace OurHeritage.Service.MappingProfile
{
    public class LikeProfile : Profile
    {
        public LikeProfile()
        {
            CreateMap<CreateLikeDto, Like>()
                .ForMember(dest => dest.LikedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.CulturalArticleId, opt => opt.MapFrom(src => src.CulturalArticleId))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ReverseMap();

            CreateMap<GetLikeDto, Like>()
                .ForMember(dest => dest.LikedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.CulturalArticleId, opt => opt.MapFrom(src => src.CulturalArticleId))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => $"{dest.User.FirstName} {dest.User.LastName}", opt => opt.MapFrom(src =>src.NameOfUser))
                .ForMember(dest => dest.User.ProfilePicture, opt => opt.MapFrom(src =>src.UserProfilePicture))
                
                .ReverseMap();
        }
    }
}
