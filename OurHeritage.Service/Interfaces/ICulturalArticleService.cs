using OurHeritage.Core.Entities;
using OurHeritage.Core.Specifications;
using OurHeritage.Service.DTOs;
using OurHeritage.Service.DTOs.CulturalArticleDto;
using System.Linq.Expressions;

namespace OurHeritage.Service.Interfaces
{
    public interface ICulturalArticleService
    {
        Task<GenericResponseDto<CulturalArticleStatisticsDto>> GetCulturalArticleStatisticsAsync(int culturalArticleId);
        Task<ResponseDto> GetAllCulturalArticlesAsync();
        Task<ResponseDto> GetUserFeedAsync(int userId);
        Task<ResponseDto> GetCulturalArticleByIdAsync(int id, int currentUserId);
        Task<ResponseDto> GetCulturalArticlesWithSpecAsync(ISpecification<CulturalArticle> spec);
        Task<ResponseDto> FindCulturalArticleAsync(Expression<Func<CulturalArticle, bool>> predicate);
        Task<ResponseDto> GetCulturalArticlesByPredicateAsync(Expression<Func<CulturalArticle, bool>> predicate, string[] includes = null);
        Task<ResponseDto> AddCulturalArticleAsync(CreateOrUpdateCulturalArticleDto createCulturalArticleDto);
        Task<ResponseDto> UpdateCulturalArticleAsync(int id, CreateOrUpdateCulturalArticleDto updateCulturalArticleDto);
        Task<ResponseDto> DeleteCulturalArticleAsync(int id);

        Task<ResponseDto> GetUserArticlesAsync(int userId);

        Task<ArticleStatsDto> GetArticleStatsAsync(int articleId);
    }
}
