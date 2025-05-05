using OurHeritage.Core.Enums;

namespace OurHeritage.Service.DTOs.ChatDto
{
    public class ReplyMessageDto
    {
        public int ConversationId { get; set; }
        public string Content { get; set; }
        public MessageType Type { get; set; } = MessageType.Text;
        public string? Attachment { get; set; }
        public int ReplyToMessageId { get; set; } // ID of the message being replied to
    }
}
