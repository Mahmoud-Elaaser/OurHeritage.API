using AutoMapper;
using OurHeritage.Core.Entities;
using OurHeritage.Service.DTOs.HandiCraftDto;

namespace OurHeritage.Service.MappingProfile
{
    public class HandiCraftProfile : Profile
    {
        public HandiCraftProfile()
        {
            CreateMap<CreateOrUpdateHandiCraftDto, HandiCraft>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
                .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId))
                .ForMember(dest => dest.ImageOrVideo, opt => opt.MapFrom(src => src.ImageOrVideo))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ReverseMap();


            CreateMap<GetHandiCraftDto, HandiCraft>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
                .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId))
                .ForMember(dest => dest.ImageOrVideo, opt => opt.MapFrom(src => src.ImageOrVideo))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ReverseMap();
        }
    }
}
