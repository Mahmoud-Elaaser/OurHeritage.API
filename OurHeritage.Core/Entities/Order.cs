using OurHeritage.Core.Enums;

namespace OurHeritage.Core.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public string Address { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public double TotalPrice { get; set; }
        public string StripePaymentIntentId { get; set; }
        //public string StripeClientSecret { get; set; }
        public OrderStatus OrderStatus { get; set; } = OrderStatus.Pending;
        public bool IsPaid { get; set; }
        public ICollection<HandiCraft> HandiCrafts { get; set; } = new List<HandiCraft>();
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }

}
