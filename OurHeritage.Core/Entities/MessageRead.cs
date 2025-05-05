namespace OurHeritage.Core.Entities
{
    public class MessageRead
    {
        public int Id { get; set; }
        public int MessageId { get; set; }
        public int UserId { get; set; }
        public DateTime ReadAt { get; set; } = DateTime.UtcNow;

        public Message Message { get; set; }
        public User User { get; set; }
    }
}
