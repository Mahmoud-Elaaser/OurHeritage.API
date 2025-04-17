using System.ComponentModel.DataAnnotations;

namespace OurHeritage.Service.DTOs.StoryDto
{
    public class CreateOrUpdateStoryDto
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }
    }
}
