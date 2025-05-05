using OurHeritage.Core.Enums;

namespace OurHeritage.Service.DTOs.ChatDto
{
    public class MessageDto
    {
        public int Id { get; set; }
        public int ConversationId { get; set; }
        public UserPreviewDto Sender { get; set; }
        public string Content { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }
        public MessageType Type { get; set; }
        public string Attachment { get; set; }
        public List<UserPreviewDto> ReadBy { get; set; }

        public int? ReplyToMessageId { get; set; }
        public ReplyPreviewDto ReplyToMessage { get; set; }
    }
}
