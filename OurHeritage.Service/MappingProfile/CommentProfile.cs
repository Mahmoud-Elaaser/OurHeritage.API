using AutoMapper;
using OurHeritage.Core.Entities;
using OurHeritage.Service.DTOs.CommentDto;

namespace OurHeritage.Service.MappingProfile
{
    public class CommentProfile : Profile
    {
        public CommentProfile()
        {
            CreateMap<GetCommentDto, Comment>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.CulturalArticleId, opt => opt.MapFrom(src => src.CulturalArticleId))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content))
                .ForMember(dest => dest.DateCreated, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ReverseMap();

            CreateMap<CreateOrUpdateCommentDto, Comment>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.CulturalArticleId, opt => opt.MapFrom(src => src.CulturalArticleId))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content))
                .ForMember(dest => dest.DateCreated, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ReverseMap();


        }
    }
}