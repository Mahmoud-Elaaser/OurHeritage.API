using OurHeritage.Service.DTOs;
using OurHeritage.Service.DTOs.HandiCraftDto;
using System.Security.Claims;

namespace OurHeritage.Service.Interfaces
{
    public interface IHandiCraftService
    {
        Task<ResponseDto> CreateHandiCraftAsync(CreateOrUpdateHandiCraftDto dto);
        Task<ResponseDto> GetHandiCraftByIdAsync(int HandiCraftId);
        Task<ResponseDto> GetAllHandiCraftsAsync();
        Task<ResponseDto> UpdateHandiCraftAsync(int HandiCraftId, CreateOrUpdateHandiCraftDto dto);
        Task<ResponseDto> DeleteHandiCraftAsync(ClaimsPrincipal user, int handiCraftId);
    }
}
