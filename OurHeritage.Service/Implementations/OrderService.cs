using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OurHeritage.Core.Entities;
using OurHeritage.Core.Enums;
using OurHeritage.Repo.Repositories.Interfaces;
using OurHeritage.Service.DTOs;
using OurHeritage.Service.DTOs.OrderDto;
using OurHeritage.Service.Interfaces;
using Stripe;

namespace OurHeritage.Service.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPaymentService _paymentService;
        private readonly ILogger<OrderService> _logger;
        private readonly IConfiguration _configuration;

        public OrderService(IUnitOfWork unitOfWork, IPaymentService paymentService, ILogger<OrderService> logger, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _paymentService = paymentService;
            _logger = logger;
            _configuration = configuration;

            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
        }

        public async Task<ResponseDto> CreateOrderAsync(CreateOrderDto orderDto)
        {
            try
            {
                var includes = new string[] { "HandiCraft" };
                var basketItems = await _unitOfWork.Repository<BasketItem>()
                    .GetAllPredicated(b => b.UserId == orderDto.UserId, includes);
                if (basketItems == null || !basketItems.Any())
                {
                    return new ResponseDto
                    {
                        IsSucceeded = false,
                        Message = "No items in basket to create order"
                    };
                }

                // Check if any basket item quantity exceeds available stock [TODO]


                var orderItems = basketItems.Select(b => new OrderItem
                {
                    HandiCraftId = b.HandiCraftId,
                    Quantity = b.Quantity,
                    Price = b.HandiCraft.Price
                }).ToList();

                double totalPrice = orderItems.Sum(item => item.Price * item.Quantity);

                // Create a PaymentIntent using Stripe's API
                var options = new PaymentIntentCreateOptions
                {
                    Amount = (long)(totalPrice * 100),  // Stripe works with the smallest currency unit, e.g., cents
                    Currency = "usd",
                    PaymentMethodTypes = new List<string> { "card" },
                };
                var service = new PaymentIntentService();
                var paymentIntent = await service.CreateAsync(options);

                var order = new Order
                {
                    UserId = orderDto.UserId,
                    OrderDate = DateTime.UtcNow,
                    OrderStatus = OrderStatus.Pending,
                    Address = orderDto.ShippingAddress,
                    IsPaid = false,
                    TotalPrice = totalPrice,
                    StripePaymentIntentId = paymentIntent.Id,
                    OrderItems = orderItems
                };
                await _unitOfWork.Repository<Order>().AddAsync(order);
                // Clear basket after making the order
                foreach (var item in basketItems)
                {
                    _unitOfWork.Repository<BasketItem>().Delete(item);
                }
                await _unitOfWork.CompleteAsync();



                var orderResponseDto = MapOrderToResponseDto(order);

                return new ResponseDto
                {
                    IsSucceeded = true,
                    Message = "Order created successfully",
                    Model = orderResponseDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order for user {UserId}", orderDto.UserId);
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Message = "An error occurred while creating the order"
                };
            }
        }
        public async Task<ResponseDto> GetOrdersForUserAsync(int userId)
        {
            try
            {
                var includes = new string[] { "OrderItems.HandiCraft" };
                var orders = await _unitOfWork.Repository<Order>()
                    .GetAllPredicated(o => o.UserId == userId, includes);

                if (orders == null || !orders.Any())
                {
                    return new ResponseDto
                    {
                        IsSucceeded = false,
                        Message = "No orders found for this user"
                    };
                }

                var orderResponseDtos = orders
                    .Select(order => MapOrderToResponseDto(order))
                    .ToList();

                return new ResponseDto
                {
                    IsSucceeded = true,
                    Models = orderResponseDtos
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching orders for user {UserId}", userId);
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Message = "An error occurred while fetching the orders"
                };
            }
        }

        public async Task<ResponseDto> GetOrderByIdAsync(int orderId)
        {
            try
            {
                var includes = new string[] { "OrderItems.HandiCraft" };
                var order = await _unitOfWork.Repository<Order>()
                    .GetAllPredicated(o => o.Id == orderId, includes);

                var orderDetail = order.FirstOrDefault();

                if (orderDetail == null)
                {
                    return new ResponseDto
                    {
                        IsSucceeded = false,
                        Message = "Order not found"
                    };
                }

                var orderResponse = MapOrderToResponseDto(orderDetail);

                return new ResponseDto
                {
                    IsSucceeded = true,
                    Model = orderResponse
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching order with ID {OrderId}", orderId);
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Message = "An error occurred while fetching the order"
                };
            }
        }


        private OrderResponseDto MapOrderToResponseDto(Order order)
        {
            return new OrderResponseDto
            {
                Id = order.Id,
                UserId = order.UserId,
                Address = order.Address,
                OrderDate = order.OrderDate,
                TotalPrice = order.TotalPrice,
                StripePaymentIntentId = order.StripePaymentIntentId,
                OrderStatus = order.OrderStatus,
                IsPaid = order.IsPaid,
                OrderItems = order.OrderItems.Select(item => new OrderItemResponseDto
                {
                    Id = item.Id,
                    HandiCraftId = item.HandiCraftId,
                    Price = item.Price,
                    Quantity = item.Quantity,
                    HandiCraft = new HandiCraftResponseDto
                    {
                        Id = item.HandiCraft.Id,
                        Title = item.HandiCraft.Title,
                        Price = item.HandiCraft.Price,
                        ImageOrVideo = item.HandiCraft.ImageOrVideo.ToList()
                    }
                }).ToList()
            };
        }
    }
}