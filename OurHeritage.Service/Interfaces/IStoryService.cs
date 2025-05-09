using OurHeritage.Core.Specifications;
using OurHeritage.Service.DTOs;
using OurHeritage.Service.DTOs.StoryDto;
using System.Security.Claims;

namespace OurHeritage.Service.Interfaces
{
    public interface IStoryService
    {
        Task<ResponseDto> CreateStoryAsync(CreateOrUpdateStoryDto dto);
        Task<ResponseDto> GetStoryByIdAsync(int storyId);
        Task<PaginationResponse<GetStoryDto>> GetAllStoriesAsync(int pageIndex = 1, int pageSize = 10);
        Task<ResponseDto> UpdateStoryAsync(int storyId, CreateOrUpdateStoryDto dto);
        Task<ResponseDto> DeleteStoryAsync(ClaimsPrincipal user, int storyId);
    }
}
