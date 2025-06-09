using OurHeritage.Core.Enums;
using OurHeritage.Service.DTOs;

namespace OurHeritage.Service.Interfaces
{
    public interface IRecommendationService
    {
        Task<List<RecommendationResult>> GetRecommendationsAsync(int userId, RecommendationType type = RecommendationType.Mixed, int count = 10);
        Task<List<RecommendationResult>> GetCulturalRecommendationsAsync(int userId, int count = 10);
        Task<List<RecommendationResult>> GetHandicraftRecommendationsAsync(int userId, int count = 10);
        Task<List<RecommendationResult>> GetSimilarHandicraftsAsync(int handicraftId, int count = 5);
        Task<List<RecommendationResult>> GetSimilarCulturalsAsync(int culturalId, int count = 5);
        Task UpdateUserPreferencesAsync(int userId);
        Task<List<RecommendationResult>> GetTrendingHandicraftsAsync(int count = 10);
        Task<List<RecommendationResult>> GetRecentHandicraftsAsync(int count = 10);
        Task<List<RecommendationResult>> GetHandicraftsByPriceRangeAsync(int userId, double minPrice, double maxPrice, int count = 10);
    }
}
