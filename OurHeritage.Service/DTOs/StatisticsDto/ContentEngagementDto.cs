namespace OurHeritage.Service.DTOs.StatisticsDto
{
    public class ContentEngagementDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string ContentType { get; set; }
        public string CreatorName { get; set; }
        public DateTime DateCreated { get; set; }
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }
        public int FavoriteCount { get; set; }
        public int TotalEngagement { get; set; }
    }
}
