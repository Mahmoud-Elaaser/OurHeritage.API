using OurHeritage.Core.Specifications;
using OurHeritage.Service.DTOs;
using OurHeritage.Service.DTOs.RepostDto;

namespace OurHeritage.Service.Interfaces
{
    public interface IRepostService
    {
        Task<ResponseDto> AddRepostAsync(int userId, int culturalArticleId, string content = null);
        Task<PaginationResponse<GetRepostDto>> GetRepostsByCulturalArticleAsync(int culturalArticleId, int page = 1, int pageSize = 10);
        Task<int> CountRepostsAsync(int culturalArticleId);
        Task<bool> IsRepostedAsync(int userId, int culturalArticleId);
        Task<ResponseDto> RemoveRepostAsync(int repostId);
        Task<ResponseDto> GetAllRepostsOnCulturalArticleAsync(int culturalArticleId);
    }
}
