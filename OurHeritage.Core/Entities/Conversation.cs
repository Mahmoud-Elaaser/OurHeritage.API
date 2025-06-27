namespace OurHeritage.Core.Entities
{
    public class Conversation
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsGroup { get; set; } = false;
        public string? GroupPicture { get; set; }

        public ICollection<ConversationUser> Participants { get; set; } = new List<ConversationUser>();
        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
