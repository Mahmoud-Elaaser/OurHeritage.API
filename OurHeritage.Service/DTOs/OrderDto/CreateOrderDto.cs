namespace OurHeritage.Service.DTOs.OrderDto
{
    public class CreateOrderDto
    {
        public int UserId { get; set; } = 0;
        public string FullName { get; set; }
        public string? ShippingAddress { get; set; }
        public string? StripePaymentIntentId { get; set; }

        public string PaymentMethod { get; set; }
        public List<OrderItemDto> Items { get; set; }
    }

}
