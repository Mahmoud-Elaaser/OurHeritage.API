namespace OurHeritage.Core.Entities
{
    public class ConversationUser
    {
        public int UserId { get; set; }
        public int ConversationId { get; set; }
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; }
        public Conversation Conversation { get; set; }
    }
}
