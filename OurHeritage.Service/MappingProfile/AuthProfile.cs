using AutoMapper;
using OurHeritage.Core.Entities;
using OurHeritage.Service.DTOs.AuthDto;

namespace OurHeritage.Service.MappingProfile
{
    public class AuthProfile : Profile
    {
        public AuthProfile()
        {

            CreateMap<RegisterDto, User>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender))
                .ForMember(dest => dest.DateJoined, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ReverseMap();

            CreateMap<LoginDto, User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.MapFrom(src => src.Password))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ReverseMap();

        }
    }
}
