using OurHeritage.Core.Entities;

namespace OurHeritage.Service.Interfaces
{
    public interface IPaymentService
    {
        Task<bool> ProcessPaymentAsync(int userId, string paymentMethod, List<BasketItem> items);
    }

}
