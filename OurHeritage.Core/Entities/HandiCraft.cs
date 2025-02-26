namespace OurHeritage.Core.Entities
{
    public class HandiCraft
    {
        public int Id { get; set; }
       // public string Name { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public List<string> ImageOrVideo { get; set; }
        public double Price { get; set; }

        public DateTime DateAdded { get; set; } = DateTime.UtcNow;
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public ICollection<Order> Orders { get; set; }
        public ICollection<Favorite> Favorite { get; set; }
    }
}
