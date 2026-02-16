using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
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
        private static ConcurrentDictionary<string, OtpData> OtpStorage = new ConcurrentDictionary<string, OtpData>(); 

        public AuthService(UserManager<User> userManager,
                           RoleManager<IdentityRole<int>> roleManager,
                           IMapper mapper,
                           IConfiguration configuration,
                           SignInManager<User> signInManager,
                           IEmailService emailService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
            _configuration = configuration;
            _signInManager = signInManager;
            _emailService = emailService;
        }

        public async Task<ResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            var user = _mapper.Map<User>(registerDto);

            var result = await _userManager.CreateAsync(user, registerDto.Password);


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

            var generatedToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var body = $@"
                        <html>
                        <head>
                            <style>
                                body {{
                                    font-family: Arial, sans-serif;
                                    background-color: #f4f4f4;
                                    padding: 20px;
                                }}
                                .container {{
                                    background-color: #ffffff;
                                    padding: 20px;
                                    border-radius: 6px;
                                    max-width: 500px;
                                    margin: auto;
                                    border: 1px solid #dddddd;
                                }}
                                .token {{
                                    font-size: 18px;
                                    font-weight: bold;
                                    color: #2c3e50;
                                    background-color: #f0f2f5;
                                    padding: 10px;
                                    border-radius: 4px;
                                    display: inline-block;
                                }}
                                .footer {{
                                    font-size: 12px;
                                    color: #777777;
                                    margin-top: 20px;
                                }}
                            </style>
                        </head>
                        <body>
                            <div class='container'>
                                <h3>Email Confirmation</h3>
                                <p>Please use this token to confirm your email:</p>
                                <div class='token'>{generatedToken}</div>

                                <p style='margin-top:15px;'>Your ID: <strong>{user.Id}</strong></p>

                                <div class='footer'>
                                    If you did not request this email, you can safely ignore it.
                                </div>
                            </div>
                        </body>
                        </html>
                        ";

            await _emailService.SendEmailAsync(user.Email, "Confirm Your Email", body);

            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 201,
                Message = "Registration completed successfully.",
                Model = new { UserId = user.Id, Email = user.Email }
            };
        }

        public async Task<ResponseDto> ConfirmEmailAsync(ConfirmEmailDto confirmEmailDto)
        {
            var user = await _userManager.FindByIdAsync(confirmEmailDto.UserId);
            if (user == null)
            {
                return new ResponseDto
                {
                    Status = 404,
                    Message = "User not found"
                };
            }

            if (user.EmailConfirmed)
            {
                return new ResponseDto
                {
                    Status = 400,
                    Message = "Email already confirmed"
                };
            }

            var result = await _userManager.ConfirmEmailAsync(user, confirmEmailDto.Token);
            if (!result.Succeeded)
            {
                return new ResponseDto
                {
                    Status = 400,
                    Message = "Email confirmation failed."
                };
            }

            return new ResponseDto
            {
                Status = 200,
                Message = "Email confirmed successfully. You can now login."
            };
        }

        public async Task<ResponseDto> ResendConfirmationEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return new ResponseDto
                {
                    Status = 404,
                    Message = "User not found"
                };
            }

            if (user.EmailConfirmed)
            {
                return new ResponseDto
                {
                    Status = 400,
                    Message = "Email already confirmed"
                };
            }


            var generatedToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var body = $@"
                        <html>
                        <head>
                            <style>
                                body {{
                                    font-family: Arial, sans-serif;
                                    background-color: #f4f4f4;
                                    padding: 20px;
                                }}
                                .container {{
                                    background-color: #ffffff;
                                    padding: 20px;
                                    border-radius: 6px;
                                    max-width: 500px;
                                    margin: auto;
                                    border: 1px solid #dddddd;
                                }}
                                .token {{
                                    font-size: 18px;
                                    font-weight: bold;
                                    color: #2c3e50;
                                    background-color: #f0f2f5;
                                    padding: 10px;
                                    border-radius: 4px;
                                    display: inline-block;
                                }}
                                .footer {{
                                    font-size: 12px;
                                    color: #777777;
                                    margin-top: 20px;
                                }}
                            </style>
                        </head>
                        <body>
                            <div class='container'>
                                <h3>Email Confirmation</h3>
                                <p>Please use this token to confirm your email:</p>
                                <div class='token'>{generatedToken}</div>

                                <p style='margin-top:15px;'>Your ID: <strong>{user.Id}</strong></p>

                                <div class='footer'>
                                    If you did not request this email, you can safely ignore it.
                                </div>
                            </div>
                        </body>
                        </html>
                        ";

            await _emailService.SendEmailAsync(user.Email, "Confirm Your Email", body);

            return new ResponseDto
            {
                Status = 200,
                Message = "A confirmation email has been sent."
            };

        }

        public async Task<ResponseDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _userManager.FindByNameAsync(loginDto.Email);
            if(user == null)
            {
                return new ResponseDto
                {
                    Status = 40,
                    Message = "User not found"
                };
            }

            bool isConfirmed = await _userManager.IsEmailConfirmedAsync(user);
            if (!isConfirmed)
            {
                return new ResponseDto
                {
                    Status = 403,
                    Message = "Email not confirmed. Please confirm your email before logging in."
                };
            }

            bool result = await _userManager.CheckPasswordAsync(user, loginDto.Password);
            if (!result)
            {
                return new ResponseDto
                {
                    Status = 401,
                    Message = "Invalid email or password." 
                };
            }

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



        public async Task<ResponseDto> ForgotPassword(ForgotPasswordDto dto)
        {
            var response = new ResponseDto();

            try
            {
                var normalizedEmail = dto.Email.Trim().ToLower();

                var user = await _userManager.FindByNameAsync(normalizedEmail);
                if (user == null)
                {
                    response.Status = 400;
                    response.IsSucceeded = false;
                    response.Message = "Invalid email";
                    return response;
                }

                // Remove any existing OTP for this email 
                CleanupOtpForEmail(normalizedEmail);

                var otpCode = GenerateAndStoreOtp(normalizedEmail);

                var subject = "Forgot Password Request";
                var body = $"Your OTP Code is: {otpCode}\nDon't share it with anyone.\nThis code will expire in 10 minutes.";

                await _emailService.SendEmailAsync(dto.Email, subject, body);

                response.Status = 200;
                response.IsSucceeded = true;
                response.Message = "We sent you an email with OTP code, please check it";
            }
            catch (Exception ex)
            {
                response.Status = 500;
                response.IsSucceeded = false;
                response.Message = "An error occurred while processing your request";
            }

            return response;
        }


        public async Task<ResponseDto> ResetPassword(ResetPasswordDto dto)
        {
            var response = new ResponseDto();

            // Check if OTP exists and is valid
            if (!OtpStorage.TryGetValue(dto.OtpCode, out var otpData))
            {
                response.Status = 400;
                response.Message = "Invalid OTP code. Please try again!";
                return response;
            }

            if (DateTime.UtcNow > otpData.ExpiryTime)
            {
                OtpStorage.TryRemove(dto.OtpCode, out _);
                response.Status = 400;
                response.Message = "OTP code has expired. Please request a new one!";
                return response;
            }

            var user = await _userManager.FindByNameAsync(otpData.Email);
            if (user == null)
            {
                response.Status = 400;
                response.Message = "Invalid user!";
                return response;
            }

            try
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);

                if (result.Succeeded)
                {
                    // Remove used OTP
                    OtpStorage.TryRemove(dto.OtpCode, out _);

                    response.IsSucceeded = true;
                    response.Status = 200;
                    response.Message = "Password has been changed successfully";
                }
                else
                {
                    response.Status = 400;
                    response.Message = "Failed to reset password: " + string.Join(", ", result.Errors.Select(e => e.Description));
                }
            }
            catch (Exception ex)
            {
                response.Status = 500;
                response.Message = "An error occurred while resetting password";
            }

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



        private void CleanupOtpForEmail(string email)
        {
            var existingOtpKeys = OtpStorage
                .Where(kvp => kvp.Value.Email.Equals(email, StringComparison.OrdinalIgnoreCase))
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in existingOtpKeys)
            {
                OtpStorage.TryRemove(key, out _);
            }
        }

        public string GenerateAndStoreOtp(string email)
        {
            var random = new Random();
            var otpCode = random.Next(100000, 999999).ToString();

            // Ensure OTP is unique
            while (OtpStorage.ContainsKey(otpCode))
            {
                otpCode = random.Next(100000, 999999).ToString();
            }

            var otpData = new OtpData
            {
                Email = email,
                Code = otpCode,
                ExpiryTime = DateTime.UtcNow.AddMinutes(10) 
            };

            OtpStorage[otpCode] = otpData;

            return otpCode;
        }
    }


    public class OtpData
    {
        public string Email { get; set; }
        public string Code { get; set; }
        public DateTime ExpiryTime { get; set; }
    }
}