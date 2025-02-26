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
                .ReverseMap();


            CreateMap<GetHandiCraftDto, HandiCraft>()
                .ReverseMap();
        }
    }
}
