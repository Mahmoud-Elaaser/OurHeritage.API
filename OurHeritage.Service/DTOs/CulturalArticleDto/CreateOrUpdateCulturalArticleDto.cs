namespace OurHeritage.Service.DTOs.CulturalArticleDto
{
    public class CreateOrUpdateCulturalArticleDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string ImageURL { get; set; }
        public int CategoryId { get; set; }
        public int UserId { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
