using OurHeritage.Service.DTOs;
using OurHeritage.Service.DTOs.UserDto;
using System.Security.Claims;

namespace OurHeritage.Service.Interfaces
{
    public interface IUserService
    {
        Task<ResponseDto> CreateUserAsync(CreateOrUpdateUserDto dto);
        Task<ResponseDto> GetUserByIdAsync(int userId);
        Task<ResponseDto> GetAllUsersAsync();
        Task<ResponseDto> UpdateUserAsync(int userId, CreateOrUpdateUserDto dto);
        Task<ResponseDto> DeleteUserAsync(ClaimsPrincipal user, int userId);
        Task<ResponseDto> GetSuggestedFriendsAsync(int userId);

        Task<ResponseDto> UpdateProfilePictureAsync(int userId, UpdateProfilePicDto imageProfileDto);
        Task<ResponseDto> UpdateCoverPhotoAsync(int userId, UpdateCoverPhotoDto coverPhotoDto);

        Task<ResponseDto> AddUserSkillAsync(int userId, string skill);
        Task<ResponseDto> UpdateUserSkillAsync(int userId, string oldSkill, string newSkill);
        Task<ResponseDto> RemoveUserSkillAsync(int userId, string skill);
        Task<ResponseDto> GetUserSkillsAsync(int userId);
        Task<ResponseDto> GetUsersBySkillAsync(string skill);
    }
}
