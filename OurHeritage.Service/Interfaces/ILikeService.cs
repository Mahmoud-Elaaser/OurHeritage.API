using OurHeritage.Service.DTOs;
using OurHeritage.Service.DTOs.CulturalArticleDto;
using OurHeritage.Service.DTOs.LikeDto;
using OurHeritage.Service.DTOs.UserDto;

namespace OurHeritage.Service.Interfaces
{
    public interface ILikeService
    {
        Task<IEnumerable<GetUserDto>> GetUsersWhoLikedArticleAsync(int culturalArticleIdtId);
        Task<IEnumerable<GetCulturalArticleDto>> GetLikedArticlesAsync(int userId);
        Task<int> CountLikesAsync(int culturalArticleId);
        Task<bool> IsLikedAsync(int culturalArticleId, int userId);
        Task<ResponseDto> AddLikeAsync(CreateLikeDto addLikeDto);
        Task<ResponseDto> RemoveLikeAsync(int culturalArticleId, int userId);
        Task<ResponseDto> GetAllLikesOnCulturalArticleAsync(int culturalArticleId);


    }
}
