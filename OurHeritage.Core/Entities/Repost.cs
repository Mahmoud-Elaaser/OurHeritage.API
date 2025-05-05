namespace OurHeritage.Core.Entities
{
    public class Repost
    {
        public int Id { get; set; }
        public string? Content { get; set; }
        public int UserId { get; set; }
        public int CulturalArticleId { get; set; }

        public User? User { get; set; }
        public CulturalArticle? CulturalArticle { get; set; }
    }
}
