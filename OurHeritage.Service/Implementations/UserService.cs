using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OurHeritage.Core.Context;
using OurHeritage.Core.Entities;
using OurHeritage.Repo.Repositories.Interfaces;
using OurHeritage.Service.DTOs;
using OurHeritage.Service.DTOs.UserDto;
using OurHeritage.Service.Helper;
using OurHeritage.Service.Interfaces;
using System.Linq.Expressions;
using System.Security.Claims;

namespace OurHeritage.Service.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;

        public UserService(UserManager<User> userManager, IUnitOfWork unitOfWork, IMapper mapper, ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
            _context = context;
        }

        public async Task<ResponseDto> CreateUserAsync(CreateOrUpdateUserDto dto)
        {
            if (dto == null)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 400,
                    Message = "Model doesn't exist"
                };
            }
            var user = _mapper.Map<User>(dto);
            await _unitOfWork.Repository<User>().AddAsync(user);
            await _unitOfWork.CompleteAsync();
            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 201,
                Model = dto,
                Message = "User created successfully"
            };
        }

        public async Task<ResponseDto> GetUserByIdAsync(int userId)
        {
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
            if (user == null)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 404,
                    Message = "User not found"
                };
            }
            var userDto = _mapper.Map<GetUserDto>(user);
            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Model = userDto
            };
        }

        public async Task<ResponseDto> GetAllUsersAsync()
        {
            var users = await _unitOfWork.Repository<User>().ListAllAsync();

            var mappedUsers = _mapper.Map<IEnumerable<GetUserDto>>(users);

            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Models = mappedUsers
            };
        }

        public async Task<ResponseDto> UpdateUserAsync(int userId, CreateOrUpdateUserDto dto)
        {
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
            if (user == null)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 404,
                    Message = "User not found"
                };
            }
            if (dto.ImageProfile != null) { dto.ProfilePicture = FilesSetting.UploadFile(dto.ImageProfile, "ProfilePicture"); }
            if (dto.ImageCoverProfile != null) { dto.CoverProfilePicture = FilesSetting.UploadFile(dto.ImageCoverProfile, "ProfilePicture"); }
            _mapper.Map(dto, user);
            _unitOfWork.Repository<User>().Update(user);
            await _unitOfWork.CompleteAsync();
            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Message = "User updated successfully"
            };
        }

        public async Task<ResponseDto> UpdateProfilePictureAsync(int userId, UpdateProfilePicDto imageProfileDto)
        {
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
            if (user == null)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 404,
                    Message = "User not found"
                };
            }

            if (imageProfileDto?.ImageProfile == null)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 400,
                    Message = "Profile picture file is required"
                };
            }

            var fileUrl = FilesSetting.UploadFile(imageProfileDto.ImageProfile, "ProfilePicture");

            // delete the old image 
            if (!string.IsNullOrEmpty(user.ProfilePicture))
            {
                FilesSetting.DeleteFile(user.ProfilePicture);
            }

            user.ProfilePicture = fileUrl;
            _unitOfWork.Repository<User>().Update(user);
            await _unitOfWork.CompleteAsync();

            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Message = "Profile picture updated successfully"
            };
        }

        public async Task<ResponseDto> UpdateCoverPhotoAsync(int userId, UpdateCoverPhotoDto coverPhotoDto)
        {
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
            if (user == null)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 404,
                    Message = "User not found"
                };
            }

            if (coverPhotoDto?.ImageCover == null)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 400,
                    Message = "Cover photo file is required"
                };
            }

            if (!string.IsNullOrEmpty(user.CoverProfilePicture))
            {
                FilesSetting.DeleteFile(user.CoverProfilePicture);
            }

            user.CoverProfilePicture = FilesSetting.UploadFile(coverPhotoDto.ImageCover, "CoverPhoto");
            _unitOfWork.Repository<User>().Update(user);
            await _unitOfWork.CompleteAsync();

            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Message = "Cover photo updated successfully"
            };
        }


        public async Task<ResponseDto> DeleteUserAsync(ClaimsPrincipal currentUser, int userId)
        {
            if (!int.TryParse(currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int loggedInUserId))
            {
                return new ResponseDto { IsSucceeded = false, Status = 401, Message = "Unauthorized - User identification failed" };
            }

            var userRole = currentUser.FindFirst(ClaimTypes.Role)?.Value;
            var userToDelete = await _userManager.FindByIdAsync(userId.ToString());

            if (userToDelete == null)
            {
                return new ResponseDto { IsSucceeded = false, Status = 404, Message = "User not found" };
            }

            if (loggedInUserId != userId && userRole != "Admin")
            {
                return new ResponseDto { IsSucceeded = false, Status = 403, Message = "Forbidden - Only user or admin can perform this action" };
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. First delete notifications related to user's articles
                var articleIds = await _context.CulturalArticles
                    .Where(a => a.UserId == userId)
                    .Select(a => a.Id)
                    .ToListAsync();

                if (articleIds.Any())
                {
                    // Delete notifications first that reference these articles
                    await _context.Notifications
                        .Where(n => articleIds.Contains(n.CulturalArticleId ?? 0))
                        .ExecuteDeleteAsync();

                    // Then delete article interactions
                    await _context.Comments.Where(c => articleIds.Contains(c.CulturalArticleId)).ExecuteDeleteAsync();
                    await _context.Likes.Where(l => articleIds.Contains(l.CulturalArticleId)).ExecuteDeleteAsync();
                    await _context.Reposts.Where(r => articleIds.Contains(r.CulturalArticleId)).ExecuteDeleteAsync();

                    // Finally delete the articles
                    await _context.CulturalArticles.Where(a => a.UserId == userId).ExecuteDeleteAsync();
                }

                // 2. Handle followers/following relationships
                var followsAsFollower = await _context.Followers
                    .Where(f => f.FollowerId == userId)
                    .ToListAsync();

                if (followsAsFollower.Any())
                {
                    _context.Followers.RemoveRange(followsAsFollower);
                }

                var followsAsFollowing = await _context.Followings
                    .Where(f => f.FollowingId == userId)
                    .ToListAsync();

                if (followsAsFollowing.Any())
                {
                    _context.Followings.RemoveRange(followsAsFollowing);
                }


                // 3. Delete OrderItems related to user's HandiCrafts
                var orderItemsToDelete = await _context.OrderItems
                    .Where(oi => _context.HandiCrafts
                        .Where(h => h.UserId == userId)
                        .Select(h => h.Id)
                        .Contains(oi.HandiCraftId))
                    .ToListAsync();

                if (orderItemsToDelete.Any())
                {
                    _context.OrderItems.RemoveRange(orderItemsToDelete);
                    await _context.SaveChangesAsync();
                }

                // 4. Delete empty Orders
                var emptyOrders = await _context.Orders
                    .Where(o => !_context.OrderItems.Any(oi => oi.OrderId == o.Id))
                    .Where(o => o.UserId == userId)
                    .ToListAsync();

                if (emptyOrders.Any())
                {
                    _context.Orders.RemoveRange(emptyOrders);
                    await _context.SaveChangesAsync();
                }

                // 5. Delete remaining user-related data
                await _context.Comments.Where(c => c.UserId == userId).ExecuteDeleteAsync();
                await _context.Likes.Where(l => l.UserId == userId).ExecuteDeleteAsync();
                await _context.Favorites.Where(f => f.UserId == userId).ExecuteDeleteAsync();
                await _context.Reposts.Where(r => r.UserId == userId).ExecuteDeleteAsync();
                await _context.Notifications
                    .Where(n => n.ActorId == userId || n.RecipientId == userId)
                    .ExecuteDeleteAsync();

                // 6. Delete user's HandiCrafts and their favorites
                var handiCraftIds = await _context.HandiCrafts
                    .Where(h => h.UserId == userId)
                    .Select(h => h.Id)
                    .ToListAsync();

                if (handiCraftIds.Any())
                {
                    await _context.Favorites.Where(f => handiCraftIds.Contains(f.HandiCraftId)).ExecuteDeleteAsync();
                    await _context.HandiCrafts.Where(h => h.UserId == userId).ExecuteDeleteAsync();
                }

                // 7. Clean up profile files
                try
                {
                    if (!string.IsNullOrEmpty(userToDelete.ProfilePicture))
                        FilesSetting.DeleteFile(userToDelete.ProfilePicture);

                    if (!string.IsNullOrEmpty(userToDelete.CoverProfilePicture))
                        FilesSetting.DeleteFile(userToDelete.CoverProfilePicture);
                }
                catch (Exception ex)
                {
                    return new ResponseDto
                    {
                        IsSucceeded = false,
                        Status = 500,
                        Message = "User deletion failed."
                    };
                }

                // 8. Remove user roles
                var roles = await _userManager.GetRolesAsync(userToDelete);
                if (roles.Any())
                {
                    await _userManager.RemoveFromRolesAsync(userToDelete, roles);
                }

                // 9. Delete the user
                var result = await _userManager.DeleteAsync(userToDelete);
                if (!result.Succeeded)
                {
                    await transaction.RollbackAsync();
                    return new ResponseDto
                    {
                        IsSucceeded = false,
                        Status = 500,
                        Message = "User deletion failed: " + string.Join(", ", result.Errors.Select(e => e.Description))
                    };
                }

                await transaction.CommitAsync();
                return new ResponseDto
                {
                    IsSucceeded = true,
                    Status = 200,
                    Message = "User and all associated data deleted successfully"
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 500,
                    Message = $"An error occurred while deleting the user: {ex.Message}"
                };
            }
        }





        public async Task<ResponseDto> GetSuggestedFriendsAsync(int userId)
        {
            // Get the current user to ensure they exist
            var currentUser = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
            if (currentUser == null)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 404,
                    Message = "User not found"
                };
            }


            var allUsers = await _unitOfWork.Repository<User>().ListAllAsync();

            // Get users this person is already following
            Expression<Func<Follow, bool>> followingPredicate = f => f.FollowerId == userId;
            var followings = await _unitOfWork.Repository<Follow>().GetAllPredicated(followingPredicate, null!);

            var followingIds = followings.Select(f => f.FollowingId).ToHashSet();

            // Filter out the current user and users already being followed
            var potentialSuggestions = allUsers
                .Where(u => u.Id != userId && !followingIds.Contains(u.Id))
                .ToList();


            if (!potentialSuggestions.Any())
            {
                return new ResponseDto
                {
                    IsSucceeded = true,
                    Status = 200,
                    Models = Enumerable.Empty<GetUserDto>(),
                    Message = "No suggested friends available"
                };
            }

            // Randomly select users to make it dynamic on each page reload
            var random = new Random();

            // Shuffle the list using Fisher-Yates algorithm
            for (int i = potentialSuggestions.Count - 1; i > 0; i--)
            {
                int j = random.Next(0, i + 1);
                var temp = potentialSuggestions[i];
                potentialSuggestions[i] = potentialSuggestions[j];
                potentialSuggestions[j] = temp;
            }

            // Take requested number of suggestions (or fewer if not enough users)
            var suggestedUsers = potentialSuggestions
                .Take(Math.Min(5, potentialSuggestions.Count))
                .ToList();


            var suggestedUserDtos = _mapper.Map<IEnumerable<GetUserDto>>(suggestedUsers);

            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Models = suggestedUserDtos,
                Message = "Suggested friends retrieved successfully"
            };
        }

        public async Task<ResponseDto> AddUserSkillAsync(int userId, string skill)
        {
            if (string.IsNullOrWhiteSpace(skill))
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 400,
                    Message = "Skill name is required"
                };
            }

            var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
            if (user == null)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 404,
                    Message = "User not found"
                };
            }


            if (user.Skills == null)
            {
                user.Skills = new List<string>();
            }

            // Check if skill already exists (case-insensitive comparison)
            if (user.Skills.Any(s => s.Equals(skill, StringComparison.OrdinalIgnoreCase)))
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 400,
                    Message = "User already has this skill"
                };
            }


            user.Skills.Add(skill);
            _unitOfWork.Repository<User>().Update(user);
            await _unitOfWork.CompleteAsync();

            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 201,
                Model = skill,
                Message = "Skill added successfully"
            };
        }


        public async Task<ResponseDto> UpdateUserSkillAsync(int userId, string oldSkill, string newSkill)
        {
            if (string.IsNullOrWhiteSpace(oldSkill) || string.IsNullOrWhiteSpace(newSkill))
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 400,
                    Message = "Both old and new skill names are required"
                };
            }

            var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
            if (user == null)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 404,
                    Message = "User not found"
                };
            }


            if (user.Skills == null || !user.Skills.Any())
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 404,
                    Message = "User has no skills"
                };
            }

            // Find the index of the skill to update (case-insensitive comparison)
            int skillIndex = user.Skills.FindIndex(s => s.Equals(oldSkill, StringComparison.OrdinalIgnoreCase));
            if (skillIndex == -1)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 404,
                    Message = "Skill not found"
                };
            }

            // Check if new skill already exists
            if (user.Skills.Any(s => s.Equals(newSkill, StringComparison.OrdinalIgnoreCase) && !s.Equals(oldSkill, StringComparison.OrdinalIgnoreCase)))
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 400,
                    Message = "New skill name already exists for this user"
                };
            }

            // Update skill
            user.Skills[skillIndex] = newSkill;
            _unitOfWork.Repository<User>().Update(user);
            await _unitOfWork.CompleteAsync();

            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Model = newSkill,
                Message = "Skill updated successfully"
            };
        }


        public async Task<ResponseDto> RemoveUserSkillAsync(int userId, string skill)
        {
            if (string.IsNullOrWhiteSpace(skill))
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 400,
                    Message = "Skill name is required"
                };
            }

            var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
            if (user == null)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 404,
                    Message = "User not found"
                };
            }

            // Check if user has skills
            if (user.Skills == null || !user.Skills.Any())
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 404,
                    Message = "User has no skills"
                };
            }

            // Try to remove the skill (case-insensitive comparison)
            bool removed = false;
            for (int i = user.Skills.Count - 1; i >= 0; i--)
            {
                if (user.Skills[i].Equals(skill, StringComparison.OrdinalIgnoreCase))
                {
                    user.Skills.RemoveAt(i);
                    removed = true;
                    break;
                }
            }

            if (!removed)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 404,
                    Message = "Skill not found"
                };
            }

            _unitOfWork.Repository<User>().Update(user);
            await _unitOfWork.CompleteAsync();

            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Message = "Skill removed successfully"
            };
        }


        public async Task<ResponseDto> GetUserSkillsAsync(int userId)
        {
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
            if (user == null)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 404,
                    Message = "User not found"
                };
            }


            if (user.Skills == null || !user.Skills.Any())
            {
                return new ResponseDto
                {
                    IsSucceeded = true,
                    Status = 200,
                    Models = new List<string>(),
                    Message = "User has no skills"
                };
            }

            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Models = user.Skills,
                Message = $"Found {user.Skills.Count} skills"
            };
        }


        public async Task<ResponseDto> GetUsersBySkillAsync(string skill)
        {
            if (string.IsNullOrWhiteSpace(skill))
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 400,
                    Message = "Skill name is required"
                };
            }

            var allUsers = await _unitOfWork.Repository<User>().ListAllAsync();

            // Filter users who have the specified skill (case-insensitive comparison)
            var usersWithSkill = allUsers
                .Where(u => u.Skills != null && u.Skills.Any(s => s.Contains(skill, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            if (!usersWithSkill.Any())
            {
                return new ResponseDto
                {
                    IsSucceeded = true,
                    Status = 200,
                    Models = Enumerable.Empty<GetUserDto>(),
                    Message = "No users found with this skill"
                };
            }

            var userDtos = _mapper.Map<IEnumerable<GetUserDto>>(usersWithSkill);

            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Models = userDtos,
                Message = $"Found {userDtos.Count()} users with skill '{skill}'"
            };
        }

    }

}