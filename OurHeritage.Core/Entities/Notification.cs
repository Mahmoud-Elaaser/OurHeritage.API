using OurHeritage.Core.Enums;

namespace OurHeritage.Core.Entities
{
    public class Notification
    {
        public int Id { get; set; }
        public int? RecipientId { get; set; }
        public int ActorId { get; set; }
        public NotificationType Type { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; }
        public int? CulturalArticleId { get; set; }
        public int? CommentId { get; set; }

        public virtual User Recipient { get; set; }
        public virtual User Actor { get; set; }
        public virtual CulturalArticle CulturalArticle { get; set; }
        public virtual Comment Comment { get; set; }
    }
}
