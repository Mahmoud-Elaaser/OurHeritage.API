using OurHeritage.Service.DTOs;
using OurHeritage.Service.DTOs.FavoriteDto;

namespace OurHeritage.Service.Interfaces
{
    public interface IFavoriteService
    {
        Task<ResponseDto> AddToFavoriteAsync(AddToFavoriteDto dto);
        Task<ResponseDto> GetFavoriteByIdAsync(int FavoriteId);
        Task<ResponseDto> GetAllFavoritesAsync();
        Task<ResponseDto> DeleteFavoriteAsync(int FavoriteId);

    }
}
