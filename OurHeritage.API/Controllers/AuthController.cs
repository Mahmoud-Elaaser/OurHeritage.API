using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OurHeritage.API.Response;
using OurHeritage.Service.DTOs.AuthDto;
using OurHeritage.Service.Interfaces;
using System.Security.Claims;

namespace OurHeritage.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterDto model)
        {
            var result = await _authService.RegisterAsync(model);
            if (result == null)
                return BadRequest(new ApiResponse(result.Status, result.Message));

            return Ok(result);
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            var result = await _authService.LoginAsync(model);
            if (result == null)
                return Unauthorized(result);

            return Ok(result);
        }

        [HttpGet("get-current-user")]
        public IActionResult GetCurrentUserId()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Ok(new { userId });
        }


        [HttpPost("assign-role")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignRole([FromBody] RoleDto model)
        {
            var result = await _authService.AssignRoleAsync(model);
            if (!result.IsSucceeded)
                return BadRequest(new ApiResponse(result.Status, result.Message));

            return Ok(result);
        }

        [HttpPost("remove-role")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveRole([FromBody] RoleDto model)
        {
            var result = await _authService.RemoveRoleAsync(model);
            if (!result.IsSucceeded)
                return BadRequest(new ApiResponse(result.Status, result.Message));

            return Ok(result);
        }

        [HttpPost("update-role")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateRole([FromBody] RoleDto model)
        {
            var result = await _authService.UpdateRoleAsync(model);
            if (!result.IsSucceeded)
                return BadRequest(new ApiResponse(result.Status, result.Message));

            return Ok(result);
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto model)
        {
            var result = await _authService.ChangePasswordAsync(model);
            if (!result.IsSucceeded)
                return BadRequest(new ApiResponse(result.Status, result.Message));

            return Ok(result);
        }


        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            var response = await _authService.ForegotPassword(dto);
            if (!response.IsSucceeded)
                return BadRequest(new ApiResponse(response.Status, response.Message));
            return Ok(response);
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var response = await _authService.ResetPassword(dto);
            if (!response.IsSucceeded)
                return BadRequest(new ApiResponse(response.Status, response.Message));
            return Ok(ModelState);
        }


        [HttpPost("resend-otp")]
        public async Task<IActionResult> ResendOTPCode([FromBody] SendOTPRequest sendOTPRequest)
        {
            bool sent = await _authService.ResendOtpCode(sendOTPRequest);
            if (!sent)
                return BadRequest(new ApiResponse(400, "An error occured during resending otp-code"));
            return Ok("We sent you an email, please check it");
        }


        [HttpPost("logout")]
        public IActionResult Logout()
        {
            var response = _authService.SignOutAsync();
            return Ok(new { message = "User logged out successfully." });
        }


    }
}
