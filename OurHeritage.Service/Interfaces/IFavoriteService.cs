using OurHeritage.Core.Entities;

namespace OurHeritage.Service.Interfaces
{
    public interface IFavoriteService
    {
        Task<List<Favorite>> GetAllFavoritesAsync();
        Task<List<Favorite>> GetFavoritesByUserIdAsync(int userId);
        Task<Favorite> GetFavoriteByIdAsync(int id);
        Task<Favorite> GetFavoriteByUserAndHandiCraftAsync(int userId, int handiCraftId);
        Task<Favorite> AddFavoriteAsync(Favorite favorite);
        Task<bool> RemoveFavoriteAsync(int id);
        Task<bool> IsFavoriteAsync(int userId, int handiCraftId);

    }
}
