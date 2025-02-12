using OurHeritage.Service.DTOs;
using OurHeritage.Service.DTOs.HandiCraftDto;

namespace OurHeritage.Service.Interfaces
{
    public interface IHandiCraftService
    {
        Task<ResponseDto> CreateHandiCraftAsync(CreateOrUpdateHandiCraftDto dto);
        Task<ResponseDto> GetHandiCraftByIdAsync(int HandiCraftId);
        Task<ResponseDto> GetAllHandiCraftsAsync();
        Task<ResponseDto> UpdateHandiCraftAsync(int HandiCraftId, CreateOrUpdateHandiCraftDto dto);
        Task<ResponseDto> DeleteHandiCraftAsync(int HandiCraftId);


    }
}
