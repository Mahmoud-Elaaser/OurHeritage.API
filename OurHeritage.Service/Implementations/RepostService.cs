using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using OurHeritage.Core.Entities;
using OurHeritage.Core.Specifications;
using OurHeritage.Repo.Repositories.Interfaces;
using OurHeritage.Service.DTOs;
using OurHeritage.Service.DTOs.RepostDto;
using OurHeritage.Service.Interfaces;
using OurHeritage.Service.SignalR;

namespace OurHeritage.Service.Implementations
{
    public class RepostService : IRepostService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IPaginationService _paginationService;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly INotificationService _notificationService;

        public RepostService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IHubContext<NotificationHub> hubContext,
            INotificationService notificationService
,
            IPaginationService paginationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _hubContext = hubContext;
            _notificationService = notificationService;
            _paginationService = paginationService;
        }

        public async Task<ResponseDto> AddRepostAsync(int userId, int culturalArticleId, string content = null)
        {
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
            var article = await _unitOfWork.Repository<CulturalArticle>().GetByIdAsync(culturalArticleId);

            var existingRepost = await _unitOfWork.Repository<Repost>()
                .FindAsync(r => r.UserId == userId && r.CulturalArticleId == culturalArticleId);


            if (existingRepost != null)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 400,
                    Message = "User has already reposted this cultural article."
                };
            }

            var repost = new Repost
            {
                UserId = userId,
                CulturalArticleId = culturalArticleId,
                Content = content
            };

            await _unitOfWork.Repository<Repost>().AddAsync(repost);
            await _unitOfWork.CompleteAsync();

            string notificationMessage = $"{user.FirstName} {user.LastName} reposted your article";

            // Create notification in DB
            var notificationResult = await _notificationService.CreateArticleCommentNotificationAsync(
                actorId: userId,
                articleId: article.Id,
                message: notificationMessage);

            // Send real-time SignalR notification
            await _hubContext.Clients.User(userId.ToString())
                .SendAsync("NotifyArticleCommented", culturalArticleId, notificationMessage);

            var mappedRepost = _mapper.Map<GetRepostDto>(repost);
            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 201,
                Message = "You reposted this cultural article successfully.",
                Model = mappedRepost
            };
        }

        public async Task<PaginationResponse<GetRepostDto>> GetRepostsByCulturalArticleAsync(int culturalArticleId, int page = 1, int pageSize = 10)
        {
            var includes = new string[] { "User" };
            var reposts = await _unitOfWork.Repository<Repost>()
                .GetAllPredicated(r => r.CulturalArticleId == culturalArticleId, includes);

            var specParams = new SpecParams
            {
                PageIndex = page,
                PageSize = pageSize
            };

            // Use PaginationService to paginate and map
            return _paginationService.Paginate(reposts, specParams, repost =>
                _mapper.Map<GetRepostDto>(repost));
        }

        public async Task<int> CountRepostsAsync(int culturalArticleId)
        {
            var repostCount = await _unitOfWork.Repository<Repost>()
                .CountAsync(r => r.CulturalArticleId == culturalArticleId);

            return repostCount;
        }


        public async Task<bool> IsRepostedAsync(int userId, int culturalArticleId)
        {
            var isReposted = await _unitOfWork.Repository<Repost>()
                .AnyAsync(r => r.UserId == userId && r.CulturalArticleId == culturalArticleId);

            return isReposted == true;
        }

        public async Task<ResponseDto> RemoveRepostAsync(int repostId)
        {
            var repost = await _unitOfWork.Repository<Repost>().GetByIdAsync(repostId);
            if (repost == null)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 404,
                    Message = "Repost not found."
                };
            }

            _unitOfWork.Repository<Repost>().Delete(repost);
            await _unitOfWork.CompleteAsync();

            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Message = "Repost deleted successfully.",
            };
        }

        public async Task<ResponseDto> GetAllRepostsOnCulturalArticleAsync(int culturalArticleId)
        {
            var includes = new string[] { "User" };
            var reposts = await _unitOfWork.Repository<Repost>()
                .GetAllPredicated(r => r.CulturalArticleId == culturalArticleId, includes);

            if (reposts == null)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 404,
                    Message = "Reposts not found"
                };
            }

            var mappedReposts = _mapper.Map<IEnumerable<GetRepostDto>>(reposts);
            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Message = "Reposts retrieved successfully",
                Models = mappedReposts
            };
        }
    }
}
