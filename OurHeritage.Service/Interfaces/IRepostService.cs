using OurHeritage.Service.DTOs;

namespace OurHeritage.Service.Interfaces
{
    public interface IRepostService
    {
        Task<ResponseDto> AddRepostAsync(int userId, int culturalArticleId, string content = null);
        Task<ResponseDto> GetRepostsByCulturalArticleAsync(int culturalArticleId);
        Task<int> CountRepostsAsync(int culturalArticleId);
        Task<bool> IsRepostedAsync(int userId, int culturalArticleId);
        Task<ResponseDto> RemoveRepostAsync(int repostId);
        Task<ResponseDto> GetAllRepostsOnCulturalArticleAsync(int culturalArticleId);
    }
}
