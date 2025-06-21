using OurHeritage.Core.Enums;
using OurHeritage.Service.DTOs.RecommendationSystemDto;

namespace OurHeritage.Service.Interfaces
{
    public interface IRecommendationService
    {
        Task<RecommendationResponseDto> GetRecommendationsAsync(
            int userId,
            int pageSize = 10,
            int pageNumber = 1,
            RecommendationType filterType = RecommendationType.Both);

        Task<RecommendationResponseDto> GetRecommendationsAsync(int userId, int pageSize = 10, int pageNumber = 1);
        Task<RecommendationResponseDto> GetRecommendationsByCategoryAsync(
            int userId,
            int categoryId,
            RecommendationType filterType = RecommendationType.Both,
            int pageSize = 10,
            int pageNumber = 1);
        Task<UserEngagementDto> GetUserEngagementDataAsync(int userId);
        Task<List<RecommendationDto>> GetSimilarArticlesAsync(int articleId, int userId, int count = 5);
        Task<List<RecommendationDto>> GetSimilarHandicraftsAsync(int handicraftId, int userId, int count = 5);
        Task UpdateUserEngagementAsync(int userId, string engagementType, int itemId);
    }
}