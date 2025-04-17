using OurHeritage.Service.DTOs;
using OurHeritage.Service.DTOs.StoryDto;
using System.Security.Claims;

namespace OurHeritage.Service.Interfaces
{
    public interface IStoryService
    {
        Task<ResponseDto> CreateStoryAsync(CreateOrUpdateStoryDto dto);
        Task<ResponseDto> GetStoryByIdAsync(int storyId);
        Task<ResponseDto> GetAllStoriesAsync();
        Task<ResponseDto> UpdateStoryAsync(int storyId, CreateOrUpdateStoryDto dto);
        Task<ResponseDto> DeleteStoryAsync(ClaimsPrincipal user, int storyId);
    }
}
