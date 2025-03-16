using AutoMapper;
using OurHeritage.Core.Entities;
using OurHeritage.Service.DTOs.CulturalArticleDto;

namespace OurHeritage.Service.MappingProfile
{
    public class ArticleProfile : Profile
    {
        public ArticleProfile()
        {
            CreateMap<CreateOrUpdateCulturalArticleDto, CulturalArticle>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
               .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content))
               .ForMember(dest => dest.DateCreated, opt => opt.MapFrom(src => DateTime.UtcNow))
               .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId))
               .ForMember(dest => dest.ImageURL, opt => opt.MapFrom(src => src.ImageURL))
               .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
               .ReverseMap();

            CreateMap<GetCulturalArticleDto, CulturalArticle>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
               .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content))
               //.ForMember(dest => dest.DateCreated, opt => opt.MapFrom(src => DateTime.UtcNow))
               .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId))
               .ForMember(dest => dest.ImageURL, opt => opt.MapFrom(src => src.ImageURL))
               .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
               .ReverseMap();
        }
    }
}
