using AutoMapper;
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

        public UserService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
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


        public async Task<ResponseDto> DeleteUserAsync(ClaimsPrincipal user, int userId)
        {
            if (!int.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int loggedInUserId))
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 401,
                    Message = "User ID not found in token."
                };
            }

            // Extract user role from the token
            var loggedInUserRole = user.FindFirst(ClaimTypes.Role)?.Value;

            var userToDelete = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
            if (userToDelete == null)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 404,
                    Message = "User not found."
                };
            }

            // Check if the logged-in user is either an admin or deleting their own account
            if (loggedInUserId != userId && loggedInUserRole != "Admin")
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 403,
                    Message = "You do not have permission to delete this user."
                };
            }

            // Delete associated files
            FilesSetting.DeleteFile(userToDelete.ProfilePicture);
            FilesSetting.DeleteFile(userToDelete.CoverProfilePicture);

            _unitOfWork.Repository<User>().Delete(userToDelete);
            await _unitOfWork.CompleteAsync();

            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Message = "User deleted successfully."
            };
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



    }

}