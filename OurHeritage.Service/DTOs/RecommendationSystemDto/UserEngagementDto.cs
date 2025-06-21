namespace OurHeritage.Service.DTOs.RecommendationSystemDto
{
    public class UserEngagementDto
    {
        public int UserId { get; set; }
        public List<int> LikedArticleIds { get; set; } = new();
        public List<int> CommentedArticleIds { get; set; } = new();
        public List<int> FavoriteHandicraftIds { get; set; } = new();
        public List<int> EngagedCategoryIds { get; set; } = new();
    }
}
