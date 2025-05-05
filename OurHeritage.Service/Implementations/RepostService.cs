using AutoMapper;
using OurHeritage.Core.Entities;
using OurHeritage.Repo.Repositories.Interfaces;
using OurHeritage.Service.DTOs;
using OurHeritage.Service.DTOs.RepostDto;
using OurHeritage.Service.Interfaces;

namespace OurHeritage.Service.Implementations
{
    public class RepostService : IRepostService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public RepostService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ResponseDto> AddRepostAsync(int userId, int culturalArticleId, string content = null)
        {
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
            var mappedRepost = _mapper.Map<GetRepostDto>(repost);
            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 201,
                Message = "You reposted this cultural article successfully.",
                Model = mappedRepost
            };
        }

        public async Task<ResponseDto> GetRepostsByCulturalArticleAsync(int culturalArticleId)
        {
            var includes = new string[] { "User" };
            var reposts = await _unitOfWork.Repository<Repost>()
                .GetAllPredicated(r => r.CulturalArticleId == culturalArticleId, includes);

            var repostDtos = _mapper.Map<IEnumerable<GetRepostDto>>(reposts);

            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Message = "Reposts retrieved successfully.",
                Models = repostDtos
            };
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
