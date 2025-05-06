namespace OurHeritage.Service.DTOs.NotificationsDto
{
    public class CreateNotificationDto
    {
        public int? RecipientId { get; set; }
        public int ActorId { get; set; }
        public string Type { get; set; }
        public string Message { get; set; }
        public int? ArticleId { get; set; }
        public int? CommentId { get; set; }
    }
}
