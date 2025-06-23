using OurHeritage.Core.Entities;
using OurHeritage.Core.Specifications;
using OurHeritage.Service.DTOs;
using OurHeritage.Service.DTOs.FavoriteDto;
using System.Linq.Expressions;
using System.Security.Claims;

namespace OurHeritage.Service.Interfaces
{
    public interface IFavoriteService
    {
        Task<ResponseDto> GetUserFavoritesAsync(ClaimsPrincipal user);
        Task<ResponseDto> GetUserFavoritesAsync(int userId, SpecParams specParams);
        Task<ResponseDto> GetFavoritesForHandicraftAsync(int handicraftId, SpecParams specParams);
        Task<ResponseDto> GetFavoriteByIdAsync(int id);
        Task<ResponseDto> AddFavoriteAsync(AddToFavoriteDto createFavoriteDto);
        Task<ResponseDto> DeleteFavoriteAsync(ClaimsPrincipal user, int favoriteId);
        Task<ResponseDto> GetUserFavoritesAsync(int userId, Expression<Func<Favorite, bool>> predicate = null);
    }
}
