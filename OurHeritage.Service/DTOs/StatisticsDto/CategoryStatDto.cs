namespace OurHeritage.Service.DTOs.StatisticsDto
{
    public class CategoryStatDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int ArticleCount { get; set; }
        public int HandiCraftCount { get; set; }
        public int TotalCount { get; set; }
    }
}
