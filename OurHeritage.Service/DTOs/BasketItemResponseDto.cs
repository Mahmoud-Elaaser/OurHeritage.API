using OurHeritage.Service.DTOs.OrderDto;

namespace OurHeritage.Service.DTOs
{
    public class BasketItemResponseDto
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public int HandiCraftId { get; set; }
        public HandiCraftResponseDto HandiCraft { get; set; }
    }

}
