using OurHeritage.Service.DTOs.UserDto;

namespace OurHeritage.Service.DTOs.StatisticsDto
{
    public class AdminStatisticsDto
    {
        public int TotalUsers { get; set; }
        public int TotalCulturalArticles { get; set; }
        public int TotalHandiCrafts { get; set; }
        public int TotalLikes { get; set; }
        public int TotalComments { get; set; }
        public int TotalFavorites { get; set; }
        public int TotalFollows { get; set; }
        public int TotalCategories { get; set; }
        public int ActiveUsersToday { get; set; }
        public int NewUsersThisMonth { get; set; }
        public int ArticlesThisMonth { get; set; }
        public int HandiCraftsThisMonth { get; set; }
        public List<CategoryStatDto> CategoryStats { get; set; }
        public List<MonthlyActivityDto> MonthlyActivity { get; set; }
        public List<TopUserDto> TopActiveUsers { get; set; }
        public List<PopularContentDto> PopularArticles { get; set; }
        public List<PopularContentDto> PopularHandiCrafts { get; set; }
    }
}
