using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using OurHeritage.Core.Entities;
using OurHeritage.Service.DTOs.AuthDto;
using OurHeritage.Service.Interfaces;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace OurHeritage.Service.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly SignInManager<User> _signInManager;
        private readonly IEmailService _emailService;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<AuthService> _logger;
        private static ConcurrentDictionary<string, string> OtpStorage = new ConcurrentDictionary<string, string>(); /// for otp code

        public AuthService(UserManager<User> userManager,
                           RoleManager<IdentityRole<int>> roleManager,
                           IMapper mapper,
                           IConfiguration configuration,
                           SignInManager<User> signInManager,
                           IEmailService emailService,
                           IMemoryCache memoryCache,
                           ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
            _configuration = configuration;
            _signInManager = signInManager;
            _emailService = emailService;
            _memoryCache = memoryCache;
            _logger = logger;
        }

        public async Task<ResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            if (registerDto == null)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 400,
                    Message = "Registration data is required."
                };
            }

            var user = _mapper.Map<User>(registerDto);
            user.Email = user.Email.ToLower();
            user.UserName = user.Email;

            //// !! Manually hash the password
            user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, registerDto.Password);


            var result = await _userManager.CreateAsync(user);


            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 400,
                    Message = "User registration failed.",
                    Models = errors
                };
            }

            await _userManager.AddToRoleAsync(user, "User");

            var token = await GenerateJwtTokenAsync(user);

            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 201,
                Message = "Registration completed successfully.",
                Model = new { Token = token, UserId = user.Id, Email = user.Email }
            };
        }


        public async Task<ResponseDto> LoginAsync(LoginDto loginDto)
        {
            if (loginDto == null || string.IsNullOrWhiteSpace(loginDto.Email) || string.IsNullOrWhiteSpace(loginDto.Password))
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 400,
                    Message = "Email and password are required."
                };
            }

            var normalizedEmail = loginDto.Email.Trim().ToLower();

            //// !! FindByNameAsync 
            var user = await _userManager.FindByNameAsync(normalizedEmail);

            if (user == null)
            {
                _logger.LogWarning("Login attempt failed: User not found.");
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 400,
                    Message = "Invalid email or password."
                };
            }

            if (await _userManager.IsLockedOutAsync(user))
            {
                _logger.LogWarning($"Login attempt failed: User {user.Email} is locked out.");
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 403,
                    Message = "Your account is locked. Try again later."
                };
            }

            bool isPasswordValid = await _userManager.CheckPasswordAsync(user, loginDto.Password);
            if (!isPasswordValid)
            {
                await _userManager.AccessFailedAsync(user);
                _logger.LogWarning($"Login failed: Incorrect password for user {user.Email}");

                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 400,
                    Message = "Invalid email or password."
                };
            }

            // Reset failed attempts on successful login
            await _userManager.ResetAccessFailedCountAsync(user);

            var token = await GenerateJwtTokenAsync(user);

            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Message = "Login successful.",
                Model = new { Token = token.Token, Expiry = token.Expiry, UserId = user.Id, Email = user.Email }
            };
        }


        public async Task<ResponseDto> AssignRoleAsync(RoleDto assignedRoleDto)
        {
            var user = await _userManager.FindByIdAsync(assignedRoleDto.UserId.ToString());
            if (user == null)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 404,
                    Message = "User not found"
                };
            }

            var isExist = await _roleManager.RoleExistsAsync(assignedRoleDto.RoleName);
            if (!isExist)
                await _roleManager.CreateAsync(new IdentityRole<int> { Name = assignedRoleDto.RoleName });

            var result = await _userManager.AddToRoleAsync(user, assignedRoleDto.RoleName);
            if (!result.Succeeded)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 400,
                    Message = "something went wrong, during assigning role",
                };
            }
            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Message = "Role assigned successfully",
            };
        }

        public async Task<ResponseDto> RemoveRoleAsync(RoleDto removeRoleDto)
        {
            var user = await _userManager.FindByIdAsync(removeRoleDto.UserId.ToString());
            if (user == null)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 404,
                    Message = "Invalid Id, or this id not found"
                };
            };

            var result = await _userManager.RemoveFromRoleAsync(user, removeRoleDto.RoleName);
            if (!result.Succeeded)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 400,
                    Message = "something went wrong, during romoving role",
                };
            }
            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Message = "Role removed successfully",
            };
        }

        public async Task<ResponseDto> UpdateRoleAsync(RoleDto updateRoleDto)
        {
            var user = await _userManager.FindByIdAsync(updateRoleDto.UserId.ToString());
            if (user == null)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 404,
                    Message = "Invalid Id, or this id not found"
                };
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            var result = await AssignRoleAsync(updateRoleDto);
            if (!result.IsSucceeded)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 400,
                    Message = "something went wrong, during updating role",
                };
            }
            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Message = "Role updated successfully",
            };
        }

        public async Task<ResponseDto> ChangePasswordAsync(string userId, ChangePasswordDto changePasswordDto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 404,
                    Message = "User not found",
                };
            }

            var result = await _userManager.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);
            if (!result.Succeeded)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 400,
                    Message = "Something went wrong during changing password",
                };
            }

            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Message = "Password changed successfully",
            };
        }

        public async Task<TokenDto> GenerateJwtTokenAsync(User user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: creds);

            return new TokenDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiry = token.ValidTo
            };
        }


        public async Task SignOutAsync()
        {
            await _signInManager.SignOutAsync();
        }



        /// This function for forgot-password it takes an email and sends otp code for this email
        public async Task<ResponseDto> ForgotPassword(ForgotPasswordDto dto)
        {
            // Normalize email for consistent lookup
            var normalizedEmail = dto.Email.Trim().ToLower();

            // Find user by normalized email
            var user = await _userManager.FindByNameAsync(normalizedEmail);
            if (user == null)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 400,
                    Message = "Invalid email"
                };
            }

            // Generate reset token (you might need this later for actual password reset)
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Generate OTP code
            var otpCode = GenerateOtpCode();

            // Store OTP with normalized email for consistent retrieval
            OtpStorage[normalizedEmail] = otpCode;

            // Log for debugging
            //System.Diagnostics.Debug.WriteLine($"Stored OTP for email '{normalizedEmail}': {otpCode}");

            // Prepare email content
            var subject = "Password Reset Request";
            var body = $"Your OTP Code is: {otpCode}\n\nDon't share it with anyone.";

            // Send email
            await _emailService.SendEmailAsync(dto.Email, subject, body);

            // Set expiration for OTP (optional - prevents stale OTPs)
            SetOtpExpiration(normalizedEmail, TimeSpan.FromMinutes(10));

            return new ResponseDto
            {
                Status = 200,
                IsSucceeded = true,
                Message = "We sent you an email with OTP code, please check it"
            };
        }


        public async Task<ResponseDto> VerifyOtp(VerifyOtpDto dto)
        {
            var response = new ResponseDto();

            // Check if OTP is valid
            if (!OtpStorage.TryGetValue(dto.Email, out var otpStorage) || otpStorage != dto.OtpCode)
            {
                response.Status = 400;
                response.Message = "Invalid OTP code. Please try again!";
                return response;
            }

            // OTP is valid, generate a reset token (you can use a GUID or any secure token generation method)
            var resetToken = Guid.NewGuid().ToString();

            // Store email against the reset token
            ResetTokenStorage.Tokens[resetToken] = dto.Email;

            // Remove OTP as it's now used
            OtpStorage.TryRemove(dto.Email, out _);

            response.IsSucceeded = true;
            response.Status = 200;
            response.Message = "OTP verified successfully";
            response.Model = resetToken; // Return the token to be used in the next step

            return response;
        }


        /// This function for reset-password it takes otp code that has been sent to email then you can reset your password
        public async Task<ResponseDto> ResetPassword(ResetPasswordDto dto)
        {
            var response = new ResponseDto();

            // Validate the reset token
            if (!ResetTokenStorage.Tokens.TryGetValue(dto.ResetToken, out var email))
            {
                response.Status = 400;
                response.Message = "Invalid or expired reset token";
                return response;
            }

            var normalizedEmail = email.Trim().ToLower();
            var user = await _userManager.FindByNameAsync(normalizedEmail);

            if (user != null)
            {
                // Reset the password
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);

                if (result.Succeeded)
                {
                    // Remove the used token
                    ResetTokenStorage.Tokens.TryRemove(dto.ResetToken, out _);

                    response.IsSucceeded = true;
                    response.Status = 200;
                    response.Message = "Password has been changed successfully";
                    return response;
                }
                else
                {
                    response.Status = 400;
                    response.Message = "Failed to reset password: " + string.Join(", ", result.Errors.Select(e => e.Description));
                    return response;
                }
            }

            response.Status = 400;
            response.Message = "Invalid User!";
            return response;
        }

        private string GenerateOtpCode()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                var bytes = new byte[4];
                rng.GetBytes(bytes);
                int otp = BitConverter.ToInt32(bytes, 0) & 0x7FFFFFFF;
                return (otp % 900000 + 100000).ToString();
            }
        }

        public async Task<bool> ResendOtpCode(SendOTPRequest sendOTPRequest)
        {
            var isExist = await _userManager.FindByNameAsync(sendOTPRequest.Email);
            if (isExist == null)
            {
                return false;
            }
            string newOtp = GenerateOtpCode();
            var body = $"Your OTP Code is:\a{newOtp}\a Don't share it with anyone";
            await _emailService.SendEmailAsync(sendOTPRequest.Email, "Sending Otp code", body);
            return true;
        }


        private void SetOtpExpiration(string email, TimeSpan expirationTime)
        {
            Task.Delay(expirationTime).ContinueWith(t =>
            {
                // Remove OTP after expiration time
                if (OtpStorage.TryRemove(email, out _))
                {
                    System.Diagnostics.Debug.WriteLine($"OTP for email '{email}' has expired and was removed");
                }
            });
        }
    }
}