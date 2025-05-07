using OurHeritage.Core.Entities;
using OurHeritage.Service.DTOs.AuthDto;

namespace OurHeritage.Service.Interfaces
{
    public interface IAuthService
    {
        Task<ResponseDto> RegisterAsync(RegisterDto registerDto);
        Task<ResponseDto> LoginAsync(LoginDto loginDto);
        Task<ResponseDto> AssignRoleAsync(RoleDto dto);
        Task<ResponseDto> RemoveRoleAsync(RoleDto dto);
        Task<ResponseDto> UpdateRoleAsync(RoleDto updateRoleDto);
        Task<ResponseDto> ChangePasswordAsync(string userId, ChangePasswordDto changePasswordDto);
        Task<TokenDto> GenerateJwtTokenAsync(User user);
        Task SignOutAsync();

        Task<ResponseDto> ForgotPassword(ForgotPasswordDto dto);
        Task<ResponseDto> VerifyOtp(VerifyOtpDto dto);
        Task<ResponseDto> ResetPassword(ResetPasswordDto dto);
        Task<bool> ResendOtpCode(SendOTPRequest sendOTPRequest);
    }
}