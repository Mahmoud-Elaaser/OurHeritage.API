using OurHeritage.Core.Enums;
using OurHeritage.Service.DTOs.ChatDto;

namespace OurHeritage.Service.DTOs.NotificationsDto
{
    public class NotificationDto
    {
        public int Id { get; set; }
        public int? RecipientId { get; set; }
        public int ActorId { get; set; }
        public UserPreviewDto Actor { get; set; }
        public NotificationType Type { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
        public int? ArticleId { get; set; }
        public int? CommentId { get; set; }
    }
}
