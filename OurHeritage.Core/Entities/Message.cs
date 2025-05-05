using OurHeritage.Core.Enums;

namespace OurHeritage.Core.Entities
{
    public class Message
    {
        public int Id { get; set; }
        public int ConversationId { get; set; }
        public int SenderId { get; set; }
        public string Content { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;
        public MessageType Type { get; set; } = MessageType.Text;
        public string? Attachment { get; set; }


        public Conversation Conversation { get; set; }
        public User Sender { get; set; }
        public ICollection<MessageRead> ReadByUsers { get; set; } = new List<MessageRead>();



        public int? ReplyToMessageId { get; set; }
        public Message ReplyToMessage { get; set; }
        public ICollection<Message> Replies { get; set; } = new List<Message>();
    }
}
