using OurHeritage.Service.DTOs;

namespace OurHeritage.Service.Interfaces
{
    public interface IFollowService
    {
        Task<ResponseDto> FollowUserAsync(FollowDto createFollowDto);
        Task<ResponseDto> UnfollowUserAsync(FollowDto followDto);
        Task<ResponseDto> GetFollowersAsync(int userId);
        Task<ResponseDto> GetFollowingAsync(int userId);

    }
}
