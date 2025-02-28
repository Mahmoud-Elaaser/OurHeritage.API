using Microsoft.AspNetCore.Http;

namespace OurHeritage.Service.DTOs.HandiCraftDto
{
    public class CreateOrUpdateHandiCraftDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public List<IFormFile>? Images { get; set; }
        public List<string>? ImageOrVideo { get; set; } = new List<string>();
        public double? Price { get; set; }
        public int CategoryId { get; set; }
        public int UserId { get; set; }

    }
}
