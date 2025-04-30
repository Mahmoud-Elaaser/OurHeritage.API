using OurHeritage.Service.DTOs;

namespace OurHeritage.Service.Interfaces
{
    public interface IBasketService
    {
        Task<ResponseDto> AddToBasketAsync(int userId, int handiCraftId, int quantity);
        Task<ResponseDto> GetItemByIdAsync(int id);
        Task<ResponseDto> GetAllItemsAsync();
        Task<ResponseDto> GetItemsForUserAsync(int userId);
        Task<ResponseDto> UpdateBasketItemAsync(int basketItemId, int quantity);
        Task<ResponseDto> DeleteItemAsync(int id);
    }

}
