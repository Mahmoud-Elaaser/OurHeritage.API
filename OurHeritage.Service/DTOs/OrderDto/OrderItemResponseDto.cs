namespace OurHeritage.Service.DTOs.OrderDto
{
    public class OrderItemResponseDto
    {
        public int Id { get; set; }
        public int HandiCraftId { get; set; }
        public double Price { get; set; }
        public int Quantity { get; set; }
        public HandiCraftResponseDto HandiCraft { get; set; }
    }
}
