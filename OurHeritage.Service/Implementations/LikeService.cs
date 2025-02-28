using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using OurHeritage.Core.Entities;
using OurHeritage.Core.Specifications;
using OurHeritage.Repo.Repositories.Interfaces;
using OurHeritage.Service.DTOs;
using OurHeritage.Service.DTOs.CulturalArticleDto;
using OurHeritage.Service.DTOs.LikeDto;
using OurHeritage.Service.DTOs.UserDto;
using OurHeritage.Service.Interfaces;
using OurHeritage.Service.SignalR;

namespace OurHeritage.Service.Implementations
{
    public class LikeService : ILikeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHubContext<NotificationHub> _hubContext;

        public LikeService(IUnitOfWork unitOfWork, IMapper mapper, IHubContext<NotificationHub> hubContext)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _hubContext = hubContext;
        }


        public async Task<IEnumerable<GetUserDto>> GetUsersWhoLikedArticleAsync(int culturalArticleId)
        {
            var likes = await _unitOfWork.Repository<Like>()
                .GetAllPredicated(l => l.CulturalArticleId == culturalArticleId, new[] { "User" });

            var userIds = likes.Select(l => l.UserId).Distinct();

            var users = await _unitOfWork.Repository<User>()
                .GetAllPredicated(u => userIds.Contains(u.Id), null);

            var mappedUsers = _mapper.Map<IEnumerable<GetUserDto>>(users);


            foreach (var userDto in mappedUsers)
            {
                var user = users.FirstOrDefault(u => u.Id == userDto.Id);
                if (user != null)
                {
                    userDto.FullName = $"{user.FirstName} {user.LastName}";

                    if (string.IsNullOrEmpty(userDto.ProfilePicture))
                    {
                        userDto.ProfilePicture = "default.jpg";
                    }
                }
            }

            return mappedUsers;
        }


        public async Task<IEnumerable<GetCulturalArticleDto>> GetLikedArticlesAsync(int userId)
        {
            var likes = await _unitOfWork.Repository<Like>().GetAllPredicated(l => l.UserId == userId, null);
            var articleIds = likes.Select(l => l.CulturalArticleId).Distinct();
            var articles = await _unitOfWork.Repository<CulturalArticle>().GetAllPredicated(a => articleIds.Contains(a.Id), null);
            return _mapper.Map<IEnumerable<GetCulturalArticleDto>>(articles);
        }


        public async Task<int> CountLikesAsync(int culturalArticleId)
        {
            var specParams = new SpecParams { FilterId = culturalArticleId };
            var spec = new EntitySpecification<Like>(specParams, l => l.CulturalArticleId == culturalArticleId);
            return await _unitOfWork.Repository<Like>().CountAsync(spec);
        }

        public async Task<bool> IsLikedAsync(int culturalArticleId, int userId)
        {
            var like = await _unitOfWork.Repository<Like>().FindAsync(l =>
                l.CulturalArticleId == culturalArticleId && l.UserId == userId);
            return like != null;
        }


        public async Task<ResponseDto> AddLikeAsync(CreateLikeDto addLikeDto)
        {
            // Check if the author of the article has blocked the user
            var isBlocked = await _unitOfWork.Repository<BlockUser>().AnyAsync(b =>
                b.BlockedUserId == addLikeDto.UserId && b.BlockedById == addLikeDto.UserId);
            if (isBlocked)
            {
                return new ResponseDto
                {
                    Message = "You are blocked by the author of this article.",
                    IsSucceeded = false,
                    Status = 401
                };
            }

            // Check if the article is already liked by the user
            var existingLike = await _unitOfWork.Repository<Like>().FindAsync(l =>
                l.CulturalArticleId == addLikeDto.CulturalArticleId && l.UserId == addLikeDto.UserId);

            if (existingLike != null)
            {
                return new ResponseDto
                {
                    Message = "You already liked this article.",
                    IsSucceeded = false,
                    Status = 401
                };
            }

            var like = _mapper.Map<Like>(addLikeDto);
            like.LikedAt = DateTime.UtcNow;

            await _unitOfWork.Repository<Like>().AddAsync(like);
            await _unitOfWork.CompleteAsync();

            // SignalR Notification
            var article = await _unitOfWork.Repository<CulturalArticle>().GetByIdAsync(addLikeDto.CulturalArticleId);
            await _hubContext.Clients.User(article.UserId.ToString()).SendAsync("ReceiveNotification", $"User with id: {addLikeDto.UserId} liked your article");

            var mappedLike = _mapper.Map<CreateLikeDto>(like);
            return new ResponseDto
            {
                IsSucceeded = true,
                Message = "Like created successfully",
                Model = mappedLike
            };
        }

        public async Task<ResponseDto> RemoveLikeAsync(int culturalArticleId, int userId)
        {
            var like = await _unitOfWork.Repository<Like>().FindAsync(l =>
                l.CulturalArticleId == culturalArticleId && l.UserId == userId);

            if (like == null)
            {
                return new ResponseDto
                {
                    Message = "Article not found",
                    IsSucceeded = false,
                    Status = 404
                };
            }

            _unitOfWork.Repository<Like>().Delete(like);
            await _unitOfWork.CompleteAsync();

            return new ResponseDto
            {
                Message = "Like removed successfully",
                IsSucceeded = true,
                Status = 201
            };
        }

        public async Task<ResponseDto> GetAllLikesOnCulturalArticleAsync(int culturalArticleId)
        {
            // Include the User in the query by adding "User" to the includes array
            var likes = await _unitOfWork.Repository<Like>().GetAllPredicated(
                l => l.CulturalArticleId == culturalArticleId,
                new[] { "User" });

            if (likes == null || !likes.Any())
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 404,
                    Message = "Likes not found"
                };
            }

            var mappedLikes = _mapper.Map<IEnumerable<GetLikeDto>>(likes);

            // Set user name and profile picture for each like
            foreach (var dto in mappedLikes)
            {
                var correspondingLike = likes.FirstOrDefault(l => l.Id == dto.Id);
                if (correspondingLike != null && correspondingLike.User != null)
                {
                    dto.NameOfUser = $"{correspondingLike.User.FirstName} {correspondingLike.User.LastName}";
                    dto.UserProfilePicture = correspondingLike.User?.ProfilePicture ?? "default.jpg";
                }
                else
                {
                    dto.NameOfUser = "Unknown User";
                    dto.UserProfilePicture = "default.jpg";
                }
            }

            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Message = "Likes retrieved successfully",
                Models = mappedLikes
            };
        }

    }
}