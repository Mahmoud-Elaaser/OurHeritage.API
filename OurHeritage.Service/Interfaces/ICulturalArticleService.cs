using OurHeritage.Core.Entities;
using OurHeritage.Repo.Repositories.Interfaces;
using OurHeritage.Service.DTOs;
using OurHeritage.Service.DTOs.CulturalArticleDto;
using System.Linq.Expressions;

namespace OurHeritage.Service.Interfaces
{
    public interface ICulturalArticleService
    {
        Task<GenericResponseDto<CulturalArticleStatisticsDto>> GetCulturalArticleStatisticsAsync(int CulturalArticleId);
        Task<ResponseDto> GetAllCulturalArticlesAsync();
        Task<ResponseDto> GetUserFeedAsync(int userId);
        Task<ResponseDto> GetCulturalArticleByIdAsync(int id);
        Task<ResponseDto> GetCulturalArticlesWithSpecAsync(ISpecifications<CulturalArticle> spec);
        Task<ResponseDto> FindCulturalArticleAsync(Expression<Func<CulturalArticle, bool>> predicate);
        Task<ResponseDto> GetCulturalArticlesByPredicateAsync(Expression<Func<CulturalArticle, bool>> predicate, string[] includes = null);
        Task<ResponseDto> AddCulturalArticleAsync(CreateOrUpdateCulturalArticleDto createCulturalArticleDto);
        Task<ResponseDto> UpdateCulturalArticleAsync(int id, CreateOrUpdateCulturalArticleDto updateCulturalArticleDto);
        Task<ResponseDto> DeleteCulturalArticleAsync(int id);


    }
}
