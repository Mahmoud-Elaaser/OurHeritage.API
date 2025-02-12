namespace OurHeritage.Core.Entities
{
    public class Like
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int CulturalArticleId { get; set; }
        public CulturalArticle CulturalArticle { get; set; }
        public DateTime LikedAt { get; set; }
    }
}
