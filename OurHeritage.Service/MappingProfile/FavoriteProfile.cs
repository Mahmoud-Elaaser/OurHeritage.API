using AutoMapper;
using OurHeritage.Core.Entities;
using OurHeritage.Service.DTOs.FavoriteDto;

namespace OurHeritage.Service.MappingProfile
{
    public class FavoriteProfile : Profile
    {
        public FavoriteProfile()
        {

            CreateMap<Favorite, GetFavoriteDto>()
               .ReverseMap();



            CreateMap<AddToFavoriteDto, Favorite>()
                .ReverseMap();
        }
    }
}
