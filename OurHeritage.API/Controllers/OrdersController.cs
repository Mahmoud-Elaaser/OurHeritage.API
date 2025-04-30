using Microsoft.AspNetCore.Mvc;
using OurHeritage.API.Response;
using OurHeritage.Service.DTOs.OrderDto;
using OurHeritage.Service.Interfaces;
using System.Security.Claims;

namespace OurHeritage.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrderAsync(CreateOrderDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new ApiResponse(401, "Invalid user ID"));
            }

            dto.UserId = userId;

            var response = await _orderService.CreateOrderAsync(dto);
            if (response.IsSucceeded)
            {
                return Ok(response.Model);
            }
            return BadRequest(response);
        }

        [HttpGet("my-orders")]
        public async Task<IActionResult> GetCurrentUserOrdersAsync()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new ApiResponse(401, "Invalid user ID"));
            }

            var response = await _orderService.GetOrdersForUserAsync(userId);
            if (response.IsSucceeded)
            {
                return Ok(response.Models);
            }
            return NotFound(response);
        }

        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrderByIdAsync(int orderId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new ApiResponse(401, "Invalid user ID"));
            }

            var response = await _orderService.GetOrderByIdAsync(orderId);
            if (!response.IsSucceeded)
            {
                return NotFound(response);
            }

            // Additional security check to ensure users can only access their own orders
            if (response.Model is OurHeritage.Core.Entities.Order order && order.UserId != userId)
            {
                return Forbid();
            }

            return Ok(response.Model);
        }
    }
}