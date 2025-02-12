using AutoMapper;
using OurHeritage.Core.Entities;
using OurHeritage.Repo.Repositories.Interfaces;
using OurHeritage.Service.DTOs;
using OurHeritage.Service.DTOs.CulturalArticleDto;
using OurHeritage.Service.DTOs.LikeDto;
using OurHeritage.Service.DTOs.UserDto;
using OurHeritage.Service.Interfaces;

namespace OurHeritage.Service.Implementations
{
    public class LikeService : ILikeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public LikeService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        /// Get a list of users who liked a specific tweet
        public async Task<IEnumerable<GetUserDto>> GetUsersWhoLikedArticleAsync(int culturalArticleIdtId)
        {
            var likes = await _unitOfWork.Repository<Like>()
                .GetAllPredicated(l => l.CulturalArticleId == culturalArticleIdtId);

            var userIds = likes.Select(l => l.UserId).Distinct();

            var users = await _unitOfWork.Repository<User>()
                .GetAllPredicated(u => userIds.Contains(u.Id));
            return _mapper.Map<IEnumerable<GetUserDto>>(users);
        }

        /// Get articles liked by a specific user
        public async Task<IEnumerable<GetCulturalArticleDto>> GetLikedArticlesAsync(int userId)
        {
            var likes = await _unitOfWork.Repository<Like>().GetAllPredicated(l => l.UserId == userId);
            var tweetIds = likes.Select(l => l.CulturalArticleId).Distinct();
            var tweets = await _unitOfWork.Repository<CulturalArticle>().GetAllPredicated(t => tweetIds.Contains(t.Id));
            return _mapper.Map<IEnumerable<GetCulturalArticleDto>>(tweets);
        }

        public async Task<int> CountLikesAsync(int culturalArticleId)
        {
            return await _unitOfWork.Repository<Like>().CountAsync(l => l.CulturalArticleId == culturalArticleId);
        }

        /// Check if a user has liked a specific article
        public async Task<bool> IsLikedAsync(int culturalArticleId, int userId)
        {
            var like = await _unitOfWork.Repository<Like>().FindAsync(l =>
                l.CulturalArticleId == culturalArticleId && l.UserId == userId);
            return like != null;
        }

        public async Task<ResponseDto> AddLikeAsync(CreateLikeDto addLikeDto)
        {
            /// check if the author of the cultural article blocked you 
            var isBlocked = await _unitOfWork.Repository<BlockUser>().AnyAsync(b =>
                b.BlockedUserId == addLikeDto.UserId && b.BlockedById == addLikeDto.UserId);
            if (isBlocked)
            {
                return new ResponseDto
                {
                    Message = "You are blocked by the author of this tweet.",
                    IsSucceeded = false,
                    Status = 401
                };
            }

            var existingLike = await _unitOfWork.Repository<Like>().FindAsync(l =>
                l.CulturalArticleId == addLikeDto.CulturalArticleId && l.UserId == addLikeDto.UserId);

            /// check if article is already liked
            if (existingLike != null)
            {
                return new ResponseDto
                {
                    Message = "You already liked this tweet.",
                    IsSucceeded = false,
                    Status = 401
                };
            }

            var like = _mapper.Map<Like>(addLikeDto);
            like.LikedAt = DateTime.UtcNow;

            await _unitOfWork.Repository<Like>().AddAsync(like);
            await _unitOfWork.Complete();

            ///TODO: SignalR
            /// Notification: User with id liked your tweet

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
            await _unitOfWork.Complete();

            return new ResponseDto
            {
                Message = "Like removed successfully",
                IsSucceeded = true,
                Status = 201
            };
        }


        public async Task<ResponseDto> GetAllLikesOnCulturalArticleAsync(int culturalArticleId)
        {
            var likes = await _unitOfWork.Repository<Like>().GetAllPredicated(t => t.CulturalArticleId == culturalArticleId);

            if (likes == null)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 404,
                    Message = "Likes not found"
                };
            }

            var mappedLikes = _mapper.Map<IEnumerable<GetLikeDto>>(likes);
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
