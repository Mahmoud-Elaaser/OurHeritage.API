using AutoMapper;
using OurHeritage.Core.Entities;
using OurHeritage.Service.DTOs.RepostDto;

namespace OurHeritage.Service.MappingProfile
{
    public class RepostMappingProfile : Profile
    {
        public RepostMappingProfile()
        {
            CreateMap<Repost, GetRepostDto>();
            CreateMap<AddRepostRequest, Repost>();
        }
    }
}
