using AutoMapper;
using OurHeritage.Core.Entities;
using OurHeritage.Service.DTOs.HandiCraftDto;
using OurHeritage.Service.Helper;

namespace OurHeritage.Service.MappingProfile
{
    public class HandiCraftProfile : Profile
    {
        public HandiCraftProfile()
        {
            CreateMap<CreateOrUpdateHandiCraftDto, HandiCraft>()
                .ReverseMap();


            CreateMap<HandiCraft, GetHandiCraftDto>().ReverseMap();

        }
    }
}
