namespace OurHeritage.Service.DTOs.RecommendationSystemDto
{
    public class RecommendationResponseDto
    {
        public List<RecommendationDto> RecommendedArticles { get; set; } = new();
        public List<RecommendationDto> RecommendedHandicrafts { get; set; } = new();
        public int TotalCount { get; set; }
        public string RecommendationReason { get; set; }
    }
}
