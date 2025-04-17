using AutoMapper;
using OurHeritage.Core.Entities;
using OurHeritage.Service.DTOs.UserDto;

namespace OurHeritage.Service.MappingProfile
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<CreateOrUpdateUserDto, User>()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.Phone))
                .ForMember(dest => dest.CoverProfilePicture, opt => opt.MapFrom(src => src.CoverProfilePicture))
                .ForMember(dest => dest.ProfilePicture, opt => opt.MapFrom(src => src.ProfilePicture))
                .ForMember(dest => dest.Connections, opt => opt.MapFrom(src => src.Connections))
                .ForMember(dest => dest.Skills, opt => opt.MapFrom(src => src.Skills))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender))
                .ForMember(dest => dest.DateJoined, opt => opt.MapFrom(src => DateTime.UtcNow))

                .ReverseMap();


            CreateMap<GetUserDto, User>()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
                .ForMember(dest => dest.CoverProfilePicture, opt => opt.MapFrom(src => src.CoverProfilePicture))
                .ForMember(dest => dest.ProfilePicture, opt => opt.MapFrom(src => src.ProfilePicture))
                .ForMember(dest => dest.Connections, opt => opt.MapFrom(src => src.Connections))
                .ForMember(dest => dest.Skills, opt => opt.MapFrom(src => src.Skills))

                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender))
                .ForMember(dest => dest.DateJoined, opt => opt.MapFrom(src => src.DateJoined))
                .ReverseMap();


        }
    }
}
