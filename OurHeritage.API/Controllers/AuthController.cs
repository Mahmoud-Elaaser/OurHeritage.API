using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OurHeritage.API.Response;
using OurHeritage.Core.Entities;
using OurHeritage.Repo.Repositories.Interfaces;
using OurHeritage.Service.DTOs.AuthDto;
using OurHeritage.Service.Interfaces;
using System.Security.Claims;

namespace OurHeritage.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUnitOfWork _unitOfWork;
        public AuthController(IAuthService authService, IUnitOfWork unitOfWork)
        {
            _authService = authService;
            _unitOfWork = unitOfWork;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterUser([FromForm] RegisterDto model)
        {
            var result = await _authService.RegisterAsync(model);
            if (result == null)
                return BadRequest(new ApiResponse(result.Status, result.Message));

            return Ok(result);
        }


        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromForm] LoginDto model)
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
        public async Task<IActionResult> AssignRole([FromForm] RoleDto model)
        {
            var result = await _authService.AssignRoleAsync(model);
            if (!result.IsSucceeded)
                return BadRequest(new ApiResponse(result.Status, result.Message));

            return Ok(result.Message);
        }

        [HttpPost("remove-role")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveRole([FromForm] RoleDto model)
        {
            var result = await _authService.RemoveRoleAsync(model);
            if (!result.IsSucceeded)
                return BadRequest(new ApiResponse(result.Status, result.Message));

            return Ok(result.Message);
        }

        [HttpPost("update-role")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateRole([FromForm] RoleDto model)
        {
            var result = await _authService.UpdateRoleAsync(model);
            if (!result.IsSucceeded)
                return BadRequest(new ApiResponse(result.Status, result.Message));

            return Ok(result.Message);
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromForm] ChangePasswordDto model)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _authService.ChangePasswordAsync(userIdClaim, model);
            if (!result.IsSucceeded)
                return BadRequest(new ApiResponse(result.Status, result.Message));

            return Ok(result.Message);
        }


        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromForm] ForgotPasswordDto dto)
        {
            var response = await _authService.ForgotPassword(dto);
            if (!response.IsSucceeded)
                return BadRequest(new ApiResponse(response.Status, response.Message));
            return Ok(response.Message);
        }



        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromForm] ResetPasswordDto dto)
        {
            var response = await _authService.ResetPassword(dto);
            if (!response.IsSucceeded)
                return BadRequest(new ApiResponse(response.Status, response.Message));

            return Ok(response.Message);
        }


        [HttpPost("resend-otp")]
        public async Task<IActionResult> ResendOTPCode([FromForm] SendOTPRequest sendOTPRequest)
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

        [HttpGet("is-admin")]
        public async Task<IActionResult> IsCurrentUserAdmin()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest(new { isAdmin = false, message = "User not authenticated." });
            }

            // Get user from database
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { isAdmin = false, message = "User not found." });
            }
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var isAdmin = userRole?.ToLower() == "admin";

            return Ok(new { isAdmin = isAdmin });
        }
    }
}