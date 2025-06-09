using OurHeritage.Core.Enums;

namespace OurHeritage.Core.Entities
{
    public class UserInteraction
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int CulturalArticleId { get; set; }
        public CulturalArticle CulturalArticle { get; set; }
        public InteractionType Type { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public int Duration { get; set; } // in seconds
    }
}
