using OurHeritage.Core.Enums;

namespace OurHeritage.Service.DTOs.ChatDto
{
    public class ReplyPreviewDto
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public MessageType Type { get; set; }
        public UserPreviewDto Sender { get; set; }
    }
}
