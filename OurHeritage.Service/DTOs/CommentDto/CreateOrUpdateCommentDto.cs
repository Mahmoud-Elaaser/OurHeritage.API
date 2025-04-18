namespace OurHeritage.Service.DTOs.CommentDto
{
    public class CreateOrUpdateCommentDto
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        public int UserId { get; set; } = 0;
        public int CulturalArticleId { get; set; }
    }
}
