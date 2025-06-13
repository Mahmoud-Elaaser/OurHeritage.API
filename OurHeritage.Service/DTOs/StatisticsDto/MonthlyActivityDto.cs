namespace OurHeritage.Service.DTOs.StatisticsDto
{
    public class MonthlyActivityDto
    {
        public string Month { get; set; }
        public int Year { get; set; }
        public int NewUsers { get; set; }
        public int NewArticles { get; set; }
        public int NewHandiCrafts { get; set; }
        public int TotalLikes { get; set; }
        public int TotalComments { get; set; }
    }
}
