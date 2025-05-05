using OurHeritage.Core.Enums;

namespace OurHeritage.Service.DTOs.ChatDto
{
    public class MessagePreviewDto
    {
        public int Id { get; set; }
        public UserPreviewDto Sender { get; set; }
        public string Content { get; set; }
        public DateTime SentAt { get; set; }
        public MessageType Type { get; set; }
        public string? Attachment { get; set; }
    }
}
