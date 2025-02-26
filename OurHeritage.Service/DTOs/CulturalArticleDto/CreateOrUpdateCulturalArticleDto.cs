using Microsoft.AspNetCore.Http;

namespace OurHeritage.Service.DTOs.CulturalArticleDto
{
    public class CreateOrUpdateCulturalArticleDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public List<IFormFile>? Images { get; set; }
        public List<string>? ImageURL { get; set; } = new List<string>();
        public int CategoryId { get; set; }
        public int UserId { get; set; }
        public DateTime? DateCreated { get; set; }
    }
}
