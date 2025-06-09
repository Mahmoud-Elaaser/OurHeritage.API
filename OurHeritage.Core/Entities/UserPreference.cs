namespace OurHeritage.Core.Entities
{
    public class UserPreference
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public int Score { get; set; } // importance
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}
