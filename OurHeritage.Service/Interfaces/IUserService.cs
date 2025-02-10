using OurHeritage.Service.DTOs;
using OurHeritage.Service.DTOs.UserDto;

namespace OurHeritage.Service.Interfaces
{
    public interface IUserService
    {
        Task<ResponseDto> CreateUserAsync(CreateOrUpdateUserDto dto);
        Task<ResponseDto> GetUserByIdAsync(int userId);
        Task<ResponseDto> GetAllUsersAsync();
        Task<ResponseDto> UpdateUserAsync(int userId, CreateOrUpdateUserDto dto);
        Task<ResponseDto> DeleteUserAsync(int userId);

    }
}
