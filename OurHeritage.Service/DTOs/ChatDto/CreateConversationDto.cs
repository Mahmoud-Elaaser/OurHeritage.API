using Microsoft.AspNetCore.Http;

namespace OurHeritage.Service.DTOs.ChatDto
{
    public class CreateConversationDto
    {
        public string Title { get; set; }
        public bool IsGroup { get; set; } = false;
        public IFormFile? GroupPictureFile { get; set; }
        public List<int> ParticipantIds { get; set; }
    }
}
