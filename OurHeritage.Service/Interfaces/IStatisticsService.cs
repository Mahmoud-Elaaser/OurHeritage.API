using OurHeritage.Core.Specifications;
using OurHeritage.Service.DTOs;

namespace OurHeritage.Service.Interfaces
{
    public interface IStatisticsService
    {
        Task<ResponseDto> GetAdminStatisticsAsync();

        Task<ResponseDto> GetTopActiveUsersPaginatedAsync(SpecParams specParams);

        Task<ResponseDto> GetPopularArticlesPaginatedAsync(SpecParams specParams);


        Task<ResponseDto> GetPopularHandiCraftsPaginatedAsync(SpecParams specParams);


        Task<ResponseDto> GetCategoryStatisticsPaginatedAsync(SpecParams specParams);

        Task<ResponseDto> GetUserActivityHistoryAsync(int userId, SpecParams specParams);

        Task<ResponseDto> GetAllUsersPaginatedAsync(SpecParams specParams);

        Task<ResponseDto> GetContentEngagementPaginatedAsync(SpecParams specParams, string contentType = "all");

        Task<ResponseDto> GetUserStatisticsAsync(int userId);

        Task<ResponseDto> GetMonthlyReportAsync(int year, int month);

        Task<ResponseDto> GetCategoryStatisticsAsync();
        Task<ResponseDto> GetContentEngagementStatsAsync();
    }
}
