using OurHeritage.Service.DTOs;
using OurHeritage.Service.DTOs.OrderDto;

namespace OurHeritage.Service.Interfaces
{
    public interface IOrderService
    {
        Task<ResponseDto> CreateOrderAsync(CreateOrderDto orderDto);
        Task<ResponseDto> GetOrdersForUserAsync(int userId);
        Task<ResponseDto> GetOrderByIdAsync(int orderId);
    }

}
