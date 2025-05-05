namespace OurHeritage.Service.DTOs.RepostDto
{
    public class AddRepostRequest
    {
        public int UserId { get; set; } = 0;
        public int CulturalArticleId { get; set; }
        public string? Content { get; set; }
    }
}
