using Microsoft.EntityFrameworkCore;
using OurHeritage.Core.Context;
using OurHeritage.Core.Entities;
using OurHeritage.Service.Interfaces;

namespace OurHeritage.Service.Implementations
{
    public class FavoriteService : IFavoriteService
    {
        private readonly ApplicationDbContext _context;

        public FavoriteService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Favorite>> GetAllFavoritesAsync()
        {
            return await _context.Favorites
                .Include(f => f.User)
                .Include(f => f.HandiCraft)
                .ToListAsync();
        }

        public async Task<List<Favorite>> GetFavoritesByUserIdAsync(int userId)
        {
            return await _context.Favorites
                .Where(f => f.UserId == userId)
                .Include(f => f.HandiCraft)
                .ToListAsync();
        }

        public async Task<Favorite> GetFavoriteByIdAsync(int id)
        {
            return await _context.Favorites
                .Include(f => f.User)
                .Include(f => f.HandiCraft)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<Favorite> GetFavoriteByUserAndHandiCraftAsync(int userId, int handiCraftId)
        {
            return await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.HandiCraftId == handiCraftId);
        }

        public async Task<Favorite> AddFavoriteAsync(Favorite favorite)
        {
            var existingFavorite = await GetFavoriteByUserAndHandiCraftAsync(favorite.UserId, favorite.HandiCraftId);
            if (existingFavorite != null)
            {
                return existingFavorite;
            }

            _context.Favorites.Add(favorite);
            await _context.SaveChangesAsync();
            return favorite;
        }

        public async Task<bool> RemoveFavoriteAsync(int id)
        {
            var favorite = await _context.Favorites.FindAsync(id);
            if (favorite == null)
            {
                return false;
            }

            _context.Favorites.Remove(favorite);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsFavoriteAsync(int userId, int handiCraftId)
        {
            return await _context.Favorites
                .AnyAsync(f => f.UserId == userId && f.HandiCraftId == handiCraftId);
        }
    }
}