namespace OurHeritage.Core.Entities
{
    public class BasketItem
    {
        public int Id { get; set; }
        public int HandiCraftId { get; set; }
        public HandiCraft HandiCraft { get; set; }
        public int Quantity { get; set; } = 1;
        public int UserId { get; set; }
        public User User { get; set; }
    }

}
