using OurHeritage.Core.Entities;
using OurHeritage.Core.Specifications;
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

        public async Task<ResponseDto> GetUserFavoritesAsync(int userId, SpecParams specParams)
        {
            var spec = new EntitySpecification<Favorite>(specParams, e => e.UserId == userId);
            var entities = await _unitOfWork.Repository<Favorite>()
                .GetAllPredicated(spec.Criteria, new[] { "User", "HandiCraft", "HandiCraft.User", "HandiCraft.Category" });

            var favoriteDtos = entities.Select(favorite => new GetFavoriteDto
            {
                Id = favorite.Id,
                UserId = favorite.UserId,
                CreatorName = favorite.User != null ? $"{favorite.User.FirstName} {favorite.User.LastName}" : "Unknown User",
                CreatorProfilePicture = favorite.User?.ProfilePicture ?? "default.jpg",
                DateCreated = favorite.DateCreated.ToString("yyyy-MM-dd"),
                HandiCraft = favorite.HandiCraft != null ? new GetHandiCraftDto
                {
                    Id = favorite.HandiCraft.Id,
                    Title = favorite.HandiCraft.Title,
                    Description = favorite.HandiCraft.Description,
                    ImageOrVideo = favorite.HandiCraft.ImageOrVideo,
                    UserId = favorite.HandiCraft.UserId,
                    NameOfUser = favorite.HandiCraft.User != null
                        ? $"{favorite.HandiCraft.User.FirstName} {favorite.HandiCraft.User.LastName}"
                        : "Unknown User",
                    UserProfilePicture = favorite.HandiCraft.User?.ProfilePicture ?? "default.jpg",
                    CategoryId = favorite.HandiCraft.CategoryId,
                    CategoryName = favorite.HandiCraft.Category?.Name ?? "",
                    Price = favorite.HandiCraft.Price
                } : null
            }).ToList();

            var paginationService = new PaginationService();
            var response = paginationService.Paginate(favoriteDtos, specParams, dto => dto);

            return new ResponseDto
            {
                IsSucceeded = true,
                Message = "User favorites retrieved successfully",
                Model = response
            };
        }


        public async Task<ResponseDto> GetFavoriteByIdAsync(int id)
        {
            var favorite = (await _unitOfWork.Repository<Favorite>()
                .GetAllPredicated(x => x.Id == id, new[] { "User", "HandiCraft", "HandiCraft.User", "HandiCraft.Category" }))
                .FirstOrDefault();

            if (favorite == null)
            {
                return new ResponseDto { IsSucceeded = false, Message = "Favorite not found" };
            }

            var favoriteDto = new GetFavoriteDto
            {
                Id = favorite.Id,
                UserId = favorite.UserId,
                CreatorName = favorite.User != null ? $"{favorite.User.FirstName} {favorite.User.LastName}" : "Unknown User",
                CreatorProfilePicture = favorite.User?.ProfilePicture ?? "default.jpg",
                DateCreated = favorite.DateCreated.ToString("yyyy-MM-dd"),

                HandiCraft = favorite.HandiCraft != null ? new GetHandiCraftDto
                {
                    Id = favorite.HandiCraft.Id,
                    Title = favorite.HandiCraft.Title,
                    Description = favorite.HandiCraft.Description,
                    ImageOrVideo = favorite.HandiCraft.ImageOrVideo,
                    UserId = favorite.HandiCraft.UserId,
                    NameOfUser = favorite.HandiCraft.User != null
                        ? $"{favorite.HandiCraft.User.FirstName} {favorite.HandiCraft.User.LastName}"
                        : "Unknown User",
                    UserProfilePicture = favorite.HandiCraft.User?.ProfilePicture ?? "default.jpg",
                    CategoryId = favorite.HandiCraft.CategoryId,
                    CategoryName = favorite.HandiCraft.Category?.Name ?? "",
                    Price = favorite.HandiCraft.Price
                } : null
            };
            return new ResponseDto
            {
                IsSucceeded = true,
                Message = "Favorite retrieved successfully",
                Model = favoriteDto
            };
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

            // Get user favorites with all related data
            var favorites = await _unitOfWork.Repository<Favorite>()
                .GetAllPredicated(
                    f => f.UserId == userId,
                    new[] { "HandiCraft", "HandiCraft.User", "HandiCraft.Category", "User" }
                );

            // Transform to DTOs with flattened handicraft information
            var favoriteDtos = favorites.Select(favorite => new GetFavoriteDto
            {
                Id = favorite.Id,
                UserId = favorite.UserId,
                CreatorName = favorite.User != null
            ? $"{favorite.User.FirstName} {favorite.User.LastName}"
            : "Unknown User",
                CreatorProfilePicture = favorite.User?.ProfilePicture ?? "default.jpg",
                DateCreated = favorite.DateCreated.ToString("yyyy-MM-dd"),

                // Complete HandiCraft Information
                HandiCraft = favorite.HandiCraft != null ? new GetHandiCraftDto
                {
                    Id = favorite.HandiCraft.Id,
                    Title = favorite.HandiCraft.Title,
                    Description = favorite.HandiCraft.Description,
                    ImageOrVideo = favorite.HandiCraft.ImageOrVideo,
                    UserId = favorite.HandiCraft.UserId,
                    NameOfUser = favorite.HandiCraft.User != null
                ? $"{favorite.HandiCraft.User.FirstName} {favorite.HandiCraft.User.LastName}"
                : "Unknown User",
                    UserProfilePicture = favorite.HandiCraft.User?.ProfilePicture ?? "default.jpg",
                    CategoryId = favorite.HandiCraft.CategoryId,
                    CategoryName = favorite.HandiCraft.Category?.Name ?? "",
                    Price = favorite.HandiCraft.Price
                } : null
            }).ToList();

            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Message = $"Found {favoriteDtos.Count} favorites",
                Models = favoriteDtos
            };

        }

        public async Task<ResponseDto> GetFavoritesForHandicraftAsync(int handicraftId, SpecParams specParams)
        {
            var spec = new EntitySpecification<Favorite>(specParams, e => e.HandiCraftId == handicraftId);
            var entities = await _unitOfWork.Repository<Favorite>()
                .GetAllPredicated(spec.Criteria, new[] { "User", "HandiCraft" });

            var favoriteDtos = entities.Select(e => new GetFavoriteDto
            {
                Id = e.Id,
                UserId = e.UserId,
                HandiCraftId = e.HandiCraftId,
                HandiCraftTitle = e.HandiCraft?.Title ?? "Unknown",
                DateCreated = e.DateCreated.ToString("yyyy-MM-dd"),
                CreatorName = e.User != null ? $"{e.User.FirstName} {e.User.LastName}" : "Unknown User",
                CreatorProfilePicture = e.User?.ProfilePicture ?? "default.jpg",
            }).ToList();

            var paginationService = new PaginationService();
            var response = paginationService.Paginate(favoriteDtos, specParams, dto => dto);

            return new ResponseDto
            {
                IsSucceeded = true,
                Message = "Handicraft favorites retrieved successfully",
                Model = response
            };
        }

        public async Task<ResponseDto> AddFavoriteAsync(AddToFavoriteDto createFavoriteDto)
        {
            if (createFavoriteDto == null)
            {
                return new ResponseDto { IsSucceeded = false, Message = "Invalid input" };
            }

            // Check if the handicraft is already in the user's favorites
            var existingFavorite = (await _unitOfWork.Repository<Favorite>()
                .GetAllPredicated(f => f.UserId == createFavoriteDto.UserId && f.HandiCraftId == createFavoriteDto.HandiCraftId, null))
                .FirstOrDefault();

            if (existingFavorite != null)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 409, // Conflict status code
                    Message = "This handicraft is already in your favorites"
                };
            }


            var handicraftExists = await _unitOfWork.Repository<HandiCraft>().GetByIdAsync(createFavoriteDto.HandiCraftId);
            if (handicraftExists == null)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 404,
                    Message = "Handicraft not found"
                };
            }

            var newFavorite = new Favorite
            {
                UserId = createFavoriteDto.UserId,
                HandiCraftId = createFavoriteDto.HandiCraftId,
                DateCreated = DateTime.UtcNow
            };

            await _unitOfWork.Repository<Favorite>().AddAsync(newFavorite);
            await _unitOfWork.CompleteAsync();

            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 201,
                Message = "Favorite added successfully",
                Model = newFavorite
            };
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
                CategoryName = handiCraft.Category?.Name ?? "",
                Price = handiCraft.Price
            };

            return new ResponseDto { IsSucceeded = true, Model = handiCraftDto };
        }


    }
}
