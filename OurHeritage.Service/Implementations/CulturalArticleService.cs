using AutoMapper;
using OurHeritage.Core.Entities;
using OurHeritage.Repo.Repositories.Interfaces;
using OurHeritage.Service.DTOs;
using OurHeritage.Service.DTOs.CulturalArticleDto;
using OurHeritage.Service.Interfaces;
using System.Linq.Expressions;

namespace OurHeritage.Service.Implementations
{
    public class CulturalArticleService : ICulturalArticleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CulturalArticleService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }


        /// retrieve all statistcs of a specific article such as: number of comments, and likes
        public async Task<GenericResponseDto<CulturalArticleStatisticsDto>> GetCulturalArticleStatisticsAsync(int CulturalArticleId)
        {
            var CulturalArticle = await _unitOfWork.Repository<CulturalArticle>().GetByIdAsync(CulturalArticleId);
            if (CulturalArticle == null)
            {
                return new GenericResponseDto<CulturalArticleStatisticsDto>
                {
                    Success = false,
                    Message = "CulturalArticle not found"
                };
            }

            /// Get counts
            var likesCount = await _unitOfWork.Repository<Like>()
                .CountAsync(like => like.CulturalArticleId == CulturalArticleId);
            var commentsCount = await _unitOfWork.Repository<Comment>()
                .CountAsync(comment => comment.CulturalArticleId == CulturalArticleId);
            var CulturalArticlesCount = await _unitOfWork.Repository<CulturalArticle>()
                .CountAsync(culturalArticle => culturalArticle.Id == CulturalArticleId);

            var statisticsDto = new CulturalArticleStatisticsDto
            {
                CulturalArticleId = CulturalArticleId,
                Likes = likesCount,
                Comments = commentsCount,
            };

            return new GenericResponseDto<CulturalArticleStatisticsDto>
            {
                Success = true,
                Message = "Statistics retrieved successfully",
                Data = statisticsDto
            };
        }

        public async Task<ResponseDto> GetAllCulturalArticlesAsync()
        {
            var CulturalArticles = await _unitOfWork.Repository<CulturalArticle>().GetAllAsync();
            var mappedCulturalArticles = _mapper.Map<IEnumerable<GetCulturalArticleDto>>(CulturalArticles);
            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Models = mappedCulturalArticles
            };
        }
        public async Task<ResponseDto> GetUserFeedAsync(int userId)
        {
            /// Fetch all blocked users and filter in memory
            var allMutedUsers = await _unitOfWork.Repository<BlockUser>().GetAllAsync();
            var mutedUserIds = allMutedUsers.Where(mu => mu.BlockedById == userId).Select(mu => mu.BlockedUserId).ToList();

            /// Fetch all CulturalArticles and filter [All CulturalArticles except Blooked Users]
            var allCulturalArticles = await _unitOfWork.Repository<CulturalArticle>().GetAllAsync();
            var filteredCulturalArticles = allCulturalArticles.Where(t => !mutedUserIds.Contains(t.UserId));

            var mappedCulturalArticles = _mapper.Map<IEnumerable<GetCulturalArticleDto>>(filteredCulturalArticles);

            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Models = mappedCulturalArticles
            };
        }

        public async Task<ResponseDto> GetCulturalArticleByIdAsync(int id)
        {
            var CulturalArticle = await _unitOfWork.Repository<CulturalArticle>().GetByIdAsync(id);
            if (CulturalArticle == null)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 404,
                    Message = "CulturalArticle not found"
                };
            }

            var mappedCulturalArticle = _mapper.Map<GetCulturalArticleDto>(CulturalArticle);
            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Model = mappedCulturalArticle
            };
        }

        public async Task<ResponseDto> GetCulturalArticlesWithSpecAsync(ISpecifications<CulturalArticle> spec)
        {
            var CulturalArticles = await _unitOfWork.Repository<CulturalArticle>().GetAllAsyncWithSpec(spec);
            var mappedCulturalArticles = _mapper.Map<IEnumerable<GetCulturalArticleDto>>(CulturalArticles);
            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Models = mappedCulturalArticles
            };
        }

        public async Task<ResponseDto> FindCulturalArticleAsync(Expression<Func<CulturalArticle, bool>> predicate)
        {
            var CulturalArticle = await _unitOfWork.Repository<CulturalArticle>().FindAsync(predicate);
            if (CulturalArticle == null)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 404,
                    Message = "No predicated CulturalArticles found"
                };
            }

            var mappedCulturalArticle = _mapper.Map<GetCulturalArticleDto>(CulturalArticle);
            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Model = mappedCulturalArticle
            };
        }

        public async Task<ResponseDto> GetCulturalArticlesByPredicateAsync(Expression<Func<CulturalArticle, bool>> predicate,
            string[] includes = null)
        {
            var CulturalArticles = await _unitOfWork.Repository<CulturalArticle>().GetAllPredicated(predicate, includes);
            var mappedCulturalArticles = _mapper.Map<IEnumerable<GetCulturalArticleDto>>(CulturalArticles);
            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Models = mappedCulturalArticles
            };
        }


        public async Task<ResponseDto> AddCulturalArticleAsync(CreateOrUpdateCulturalArticleDto createCulturalArticleDto)
        {
            var CulturalArticle = _mapper.Map<CulturalArticle>(createCulturalArticleDto);
            await _unitOfWork.Repository<CulturalArticle>().AddAsync(CulturalArticle);
            await _unitOfWork.Complete();

            var mappedCulturalArticle = _mapper.Map<CreateOrUpdateCulturalArticleDto>(CulturalArticle);
            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Model = mappedCulturalArticle
            };
        }


        public async Task<ResponseDto> UpdateCulturalArticleAsync(int id, CreateOrUpdateCulturalArticleDto updateCulturalArticleDto)
        {
            var existingCulturalArticle = await _unitOfWork.Repository<CulturalArticle>().GetByIdAsync(id);
            if (existingCulturalArticle == null)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 404,
                    Message = "CulturalArticle not found"
                };
            }

            _mapper.Map(updateCulturalArticleDto, existingCulturalArticle);
            _unitOfWork.Repository<CulturalArticle>().Update(existingCulturalArticle);
            await _unitOfWork.Complete();

            var mappedCulturalArticle = _mapper.Map<GetCulturalArticleDto>(existingCulturalArticle);
            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Message = "CulturalArticle updated successfully"
            };
        }


        public async Task<ResponseDto> DeleteCulturalArticleAsync(int id)
        {
            var CulturalArticle = await _unitOfWork.Repository<CulturalArticle>().GetByIdAsync(id);
            if (CulturalArticle == null)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 404,
                    Message = "CulturalArticle not found"
                };
            }

            _unitOfWork.Repository<CulturalArticle>().Delete(CulturalArticle);
            await _unitOfWork.Complete();
            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Message = "CulturalArticle deleted successfully."
            };
        }


    }
}
