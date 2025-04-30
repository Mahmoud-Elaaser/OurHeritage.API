using Microsoft.Extensions.Logging;
using OurHeritage.Core.Entities;
using OurHeritage.Service.Interfaces;
using Stripe.Checkout;


namespace OurHeritage.Service.Implementations
{
    public class StripePaymentService : IPaymentService
    {
        private readonly ILogger<StripePaymentService> _logger;

        public StripePaymentService(ILogger<StripePaymentService> logger)
        {
            _logger = logger;
        }

        public async Task<bool> ProcessPaymentAsync(int userId, string paymentMethod, List<BasketItem> items)
        {
            try
            {
                var lineItems = items.Select(item => new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "usd",
                        UnitAmount = (long)(item.HandiCraft.Price * 100), // Stripe expects price in cents
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.HandiCraft.Title
                        }
                    },
                    Quantity = item.Quantity
                }).ToList();

                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
                    LineItems = lineItems,
                    Mode = "payment",
                    SuccessUrl = "https://ourheritage.runasp.net/payment-success",
                    CancelUrl = "https://ourheritage.runasp.net/payment-cancel"
                };

                var service = new SessionService();
                Session session = await service.CreateAsync(options);

                _logger.LogInformation("Stripe session created for user {UserId}: {SessionUrl}", userId, session.Url);


                return true; // Simulate success for now (normally you'd wait for a webhook)
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Stripe payment failed for user {UserId}", userId);
                return false;
            }
        }
    }
}
