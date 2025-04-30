using OurHeritage.Core.Enums;

namespace OurHeritage.Service.DTOs.OrderDto
{
    public class OrderResponseDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Address { get; set; }
        public DateTime OrderDate { get; set; }
        public double TotalPrice { get; set; }
        public string StripePaymentIntentId { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public bool IsPaid { get; set; }
        public List<OrderItemResponseDto> OrderItems { get; set; }
    }
}
