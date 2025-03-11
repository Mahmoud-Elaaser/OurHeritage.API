using OurHeritage.Core.Entities;
using OurHeritage.Repo.Repositories.Interfaces;
using OurHeritage.Service.DTOs;
using OurHeritage.Service.DTOs.FavoriteDto;
using OurHeritage.Service.DTOs.HandiCraftDto;
using OurHeritage.Service.Interfaces;
using System.Linq.Expressions;
using System.Security.Claims;

namespace OurHeritage.Service.Implementations
{
    public class FavoriteService : IFavoriteService
    {
        private readonly IUnitOfWork _unitOfWork;

        public FavoriteService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseDto> GetUserFavoritesAsync(ClaimsPrincipal user)
        {
            if (!int.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId))
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 401,
                    Message = "User ID not found in token."
                };
            }

            var favorites = await _unitOfWork.Repository<Favorite>()
                .GetAllPredicated(f => f.UserId == userId, new[] { "HandiCraft" });

            return new ResponseDto
            {
                IsSucceeded = true,
                Message = "Favorites retrieved successfully",
                Models = favorites
            };
        }


        public async Task<ResponseDto> GetFavoriteByIdAsync(int id)
        {
            var favorite = await _unitOfWork.Repository<Favorite>().GetByIdAsync(id);
            if (favorite == null)
            {
                return new ResponseDto { IsSucceeded = false, Message = "Favorite not found" };
            }
            return new ResponseDto
            {
                IsSucceeded = true,
                Message = "Favorite retrieved successfully",
                Model = favorite
            };
        }

        public async Task<ResponseDto> AddFavoriteAsync(AddToFavoriteDto createFavoriteDto)
        {
            if (createFavoriteDto == null)
            {
                return new ResponseDto { IsSucceeded = false, Message = "Invalid input" };
            }

            var newFavorite = new Favorite
            {
                UserId = createFavoriteDto.UserId,
                HandiCraftId = createFavoriteDto.HandiCraftId,
                DateCreated = DateTime.UtcNow
            };

            var favorite = _unitOfWork.Repository<Favorite>().GetByIdAsync(newFavorite.Id);
            if (favorite != null)
            {
                return new ResponseDto { IsSucceeded = false, Message = "You already added it to favorites" };
            }

            await _unitOfWork.Repository<Favorite>().AddAsync(newFavorite);
            await _unitOfWork.CompleteAsync();

            return new ResponseDto { IsSucceeded = true, Message = "Favorite added successfully" };
        }

        public async Task<ResponseDto> DeleteFavoriteAsync(ClaimsPrincipal user, int favoriteId)
        {
            // Extract user ID from the token
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

            var favorite = await _unitOfWork.Repository<Favorite>().GetByIdAsync(favoriteId);
            if (favorite == null)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 404,
                    Message = "Favorite not found."
                };
            }

            // Check if the logged-in user is the favorite owner or an admin
            if (loggedInUserId != favorite.UserId && loggedInUserRole != "Admin")
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 403,
                    Message = "You do not have permission to delete this favorite."
                };
            }

            _unitOfWork.Repository<Favorite>().Delete(favorite);
            await _unitOfWork.CompleteAsync();

            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Message = "Favorite deleted successfully."
            };
        }


        public async Task<ResponseDto> GetUserFavoritesAsync(int userId, Expression<Func<Favorite, bool>> predicate = null)
        {
            var userFavorites = await _unitOfWork.Repository<Favorite>().GetAllPredicated(predicate ?? (f => f.UserId == userId), new[] { "HandiCraft" });
            return new ResponseDto { IsSucceeded = true, Message = "User favorites retrieved successfully", Models = userFavorites };
        }

        public async Task<ResponseDto> GetHandiCraftDetailsAsync(int handiCraftId)
        {
            var handiCraft = (await _unitOfWork.Repository<HandiCraft>()
                .GetAllPredicated(c => c.Id == handiCraftId, new[] { "User", "Category" }))
                .FirstOrDefault();


            if (handiCraft == null)
            {
                return new ResponseDto { IsSucceeded = false, Message = "HandiCraft not found" };
            }

            var handiCraftDto = new GetHandiCraftDto
            {
                Id = handiCraft.Id,
                Title = handiCraft.Title,
                Description = handiCraft.Description,
                ImageOrVideo = handiCraft.ImageOrVideo,
                UserId = handiCraft.UserId,
                NameOfUser = handiCraft.User is not null
                    ? $"{handiCraft.User.FirstName} {handiCraft.User.LastName}"
                    : "Unknown User",
                UserProfilePicture = handiCraft.User?.ProfilePicture ?? "default.jpg",
                CategoryId = handiCraft.CategoryId,
                NameOfCategory = handiCraft.Category?.Name ?? "",
                Price = handiCraft.Price
            };

            return new ResponseDto { IsSucceeded = true, Model = handiCraftDto };
        }

    }
}
