using AutoMapper;
using OurHeritage.Core.Entities;
using OurHeritage.Service.DTOs.StoryDto;

namespace OurHeritage.Service.MappingProfile
{
    public class StoryProfile : Profile
    {
        public StoryProfile()
        {
            CreateMap<CreateOrUpdateStoryDto, Story>()
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content))
                .ReverseMap();

            CreateMap<Story, GetStoryDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content))
                .ReverseMap();
        }
    }
}
