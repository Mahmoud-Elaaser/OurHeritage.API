namespace OurHeritage.Core.Entities
{
    public class Favorite
    {

        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int HandiCraftId { get; set; }
        public HandiCraft HandiCraft { get; set; }
    }
}
