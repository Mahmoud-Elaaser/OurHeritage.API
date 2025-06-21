namespace OurHeritage.Service.DTOs.RecommendationSystemDto
{
    public class RecommendationDto
    {
        public string Type { get; set; }
        public int ItemId { get; set; }
        public string Title { get; set; }
        public List<string> Images { get; set; }
        public string Content { get; set; }
        public string CategoryName { get; set; }
        public double? Price { get; set; }
        public CreatorDto Creator { get; set; }
        public double RecommendationScore { get; set; }
    }
}
