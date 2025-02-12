namespace OurHeritage.Core.Entities
{
    public class BlockUser
    {
        public int Id { get; set; }
        public int BlockedById { get; set; } /// The user who blocked another user
        public int BlockedUserId { get; set; } /// The user being blocked

        public User BlockedBy { get; set; } /// Navigation to the blocking user
        public User BlockedUser { get; set; } /// Navigation to the blocked user
    }
}
