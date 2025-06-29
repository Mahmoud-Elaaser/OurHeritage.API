﻿using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OurHeritage.Core.Context;
using OurHeritage.Core.Entities;
using OurHeritage.Core.Specifications;
using OurHeritage.Repo.Repositories.Interfaces;
using OurHeritage.Service.DTOs;
using OurHeritage.Service.DTOs.CulturalArticleDto;
using OurHeritage.Service.Helper;
using OurHeritage.Service.Interfaces;
using System.Linq.Expressions;

namespace OurHeritage.Service.Implementations
{
    public class CulturalArticleService : ICulturalArticleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICulturalArticleRepository _articleRepository;
        private readonly ApplicationDbContext _context;

        public CulturalArticleService(IUnitOfWork unitOfWork, IMapper mapper, ICulturalArticleRepository articleRepository, ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _articleRepository = articleRepository;
            _context = context;
        }

        public async Task<GenericResponseDto<CulturalArticleStatisticsDto>> GetCulturalArticleStatisticsAsync(int culturalArticleId)
        {
            var culturalArticle = await _unitOfWork.Repository<CulturalArticle>().GetByIdAsync(culturalArticleId);
            if (culturalArticle == null)
            {
                return new GenericResponseDto<CulturalArticleStatisticsDto>
                {
                    Success = false,
                    Message = "CulturalArticle not found"
                };
            }

            var likeSpec = new EntitySpecification<Like>(new SpecParams(), l => l.CulturalArticleId == culturalArticleId);
            var commentSpec = new EntitySpecification<Comment>(new SpecParams(), c => c.CulturalArticleId == culturalArticleId);

            var likesCount = await _unitOfWork.Repository<Like>().CountAsync(likeSpec);
            var commentsCount = await _unitOfWork.Repository<Comment>().CountAsync(commentSpec);

            var statisticsDto = new CulturalArticleStatisticsDto
            {
                CulturalArticleId = culturalArticleId,
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
            var culturalArticles = await _unitOfWork.Repository<CulturalArticle>().ListAllAsync();
            var mappedCulturalArticles = _mapper.Map<IEnumerable<GetCulturalArticleDto>>(culturalArticles);
            foreach (var dto in mappedCulturalArticles)
            {
                var correspondingHandiCraft = culturalArticles.FirstOrDefault(h => h.Id == dto.Id);
                if (correspondingHandiCraft != null && correspondingHandiCraft.User != null)
                {
                    dto.NameOfUser = $"{correspondingHandiCraft.User.FirstName} {correspondingHandiCraft.User.LastName}";
                    dto.UserProfilePicture = correspondingHandiCraft.User?.ProfilePicture ?? "default.jpg";
                    dto.NameOfCategory = correspondingHandiCraft.Category.Name;
                }

            }
            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Models = mappedCulturalArticles
            };
        }

        public async Task<ResponseDto> GetUserFeedAsync(int userId)
        {
            var allMutedUsers = await _unitOfWork.Repository<BlockUser>().ListAllAsync();
            var mutedUserIds = allMutedUsers.Where(mu => mu.BlockedById == userId).Select(mu => mu.BlockedUserId).ToList();

            var allCulturalArticles = await _unitOfWork.Repository<CulturalArticle>().ListAllAsync();
            var filteredCulturalArticles = allCulturalArticles.Where(t => !mutedUserIds.Contains(t.UserId));

            var mappedCulturalArticles = _mapper.Map<IEnumerable<GetCulturalArticleDto>>(filteredCulturalArticles);

            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Models = mappedCulturalArticles
            };
        }

        public async Task<ResponseDto> GetCulturalArticleByIdAsync(int id, int currentUserId)
        {
            // Fetch only the first matching article 
            var article = (await _unitOfWork.Repository<CulturalArticle>()
                .GetAllPredicated(a => a.Id == id, new[] { "User", "Category", "Likes", "Comments" }))
                .FirstOrDefault();


            if (article == null)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 404,
                    Message = "Cultural Article not found"
                };
            }

            bool isFollowing = await _unitOfWork.Repository<Follow>()
                .AnyAsync(f => f.FollowerId == currentUserId && f.FollowingId == article.UserId);


            var articleDto = new GetCulturalArticleDto
            {
                Id = article.Id,
                Title = article.Title,
                Content = article.Content,
                UserId = article.UserId,
                CategoryId = article.CategoryId,
                ImageURL = article.ImageURL,
                NameOfUser = article.User != null ? $"{article.User.FirstName} {article.User.LastName}" : "Unknown User",
                UserProfilePicture = article.User?.ProfilePicture ?? "default.jpg",
                NameOfCategory = article.Category?.Name ?? "Unknown Category",
                DateCreated = article.DateCreated.ToString("yyyy-MM-dd"),
                TimeAgo = TimeAgoHelper.GetTimeAgo(article.DateCreated),
                LikeCount = article.Likes?.Count ?? 0,
                CommentCount = article.Comments?.Count ?? 0,
                IsFollowing = isFollowing
            };

            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Model = articleDto
            };
        }

        public async Task<ResponseDto> GetCulturalArticlesWithSpecAsync(ISpecification<CulturalArticle> spec)
        {
            var culturalArticles = await _unitOfWork.Repository<CulturalArticle>().ListAsync(spec);
            var mappedCulturalArticles = _mapper.Map<IEnumerable<GetCulturalArticleDto>>(culturalArticles);
            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Models = mappedCulturalArticles
            };
        }

        public async Task<ResponseDto> FindCulturalArticleAsync(Expression<Func<CulturalArticle, bool>> predicate)
        {
            var culturalArticle = await _unitOfWork.Repository<CulturalArticle>().FindAsync(predicate);
            if (culturalArticle == null)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 404,
                    Message = "No predicated CulturalArticles found"
                };
            }

            var mappedCulturalArticle = _mapper.Map<GetCulturalArticleDto>(culturalArticle);
            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Model = mappedCulturalArticle
            };
        }

        public async Task<ResponseDto> GetCulturalArticlesByPredicateAsync(Expression<Func<CulturalArticle, bool>> predicate, string[] includes = null)
        {
            var culturalArticles = await _unitOfWork.Repository<CulturalArticle>().GetAllPredicated(predicate, includes);
            var mappedCulturalArticles = _mapper.Map<IEnumerable<GetCulturalArticleDto>>(culturalArticles);
            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Models = mappedCulturalArticles
            };
        }

        public async Task<ResponseDto> AddCulturalArticleAsync(CreateOrUpdateCulturalArticleDto createCulturalArticleDto)
        {
            if (createCulturalArticleDto.Images != null)
            {
                foreach (var image in createCulturalArticleDto.Images)
                {
                    var uploadedFiles = FilesSetting.UploadFile(image, "CulturalArticle");
                    if (uploadedFiles != null && uploadedFiles.Any())
                    {
                        createCulturalArticleDto.ImageURL.Add(uploadedFiles);
                    }
                    else
                    {
                        return new ResponseDto
                        {
                            IsSucceeded = false,
                            Status = 401,
                            Message = "Please upload a valid file."
                        };

                    }
                }
            }
            var culturalArticle = _mapper.Map<CulturalArticle>(createCulturalArticleDto);
            await _unitOfWork.Repository<CulturalArticle>().AddAsync(culturalArticle);
            await _unitOfWork.CompleteAsync();

            var mappedCulturalArticle = _mapper.Map<CreateOrUpdateCulturalArticleDto>(culturalArticle);
            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 201,
                Model = mappedCulturalArticle,
                Message = "CulturalArticle created successfully"
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
            if (updateCulturalArticleDto.Images != null)
            {
                foreach (var imageUrl in existingCulturalArticle.ImageURL)
                {
                    FilesSetting.DeleteFile(imageUrl);

                }
                foreach (var image in updateCulturalArticleDto.Images)
                {
                    var uploadedFiles = FilesSetting.UploadFile(image, "CulturalArticle");
                    if (uploadedFiles != null && uploadedFiles.Any())
                    {
                        updateCulturalArticleDto.ImageURL.Add(uploadedFiles);
                    }
                    else
                    {
                        return new ResponseDto
                        {
                            IsSucceeded = false,
                            Status = 401,
                            Message = "Please upload a valid file."
                        };

                    }
                }
            }

            _mapper.Map(updateCulturalArticleDto, existingCulturalArticle);
            _unitOfWork.Repository<CulturalArticle>().Update(existingCulturalArticle);
            await _unitOfWork.CompleteAsync();

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
            try
            {
                var culturalArticle = await _context.CulturalArticles
                    .Include(a => a.Comments)
                    .Include(a => a.Likes)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (culturalArticle == null)
                {
                    return new ResponseDto
                    {
                        IsSucceeded = false,
                        Status = 404,
                        Message = "CulturalArticle not found"
                    };
                }

                if (culturalArticle.Comments != null && culturalArticle.Comments.Any())
                {
                    _context.Comments.RemoveRange(culturalArticle.Comments);
                }

                if (culturalArticle.Likes != null && culturalArticle.Likes.Any())
                {
                    _context.Likes.RemoveRange(culturalArticle.Likes);
                }

                var reposts = await _context.Reposts
                    .Where(r => r.CulturalArticleId == id)
                    .ToListAsync();

                if (reposts.Any())
                {
                    _context.Reposts.RemoveRange(reposts);
                }
                var notifications = await _context.Notifications
                    .Where(r => r.CulturalArticleId == id)
                    .ToListAsync();

                if (notifications.Any())
                {
                    _context.Notifications.RemoveRange(notifications);
                }

                _unitOfWork.Repository<CulturalArticle>().Delete(culturalArticle);

                await _unitOfWork.CompleteAsync();

                return new ResponseDto
                {
                    IsSucceeded = true,
                    Status = 200,
                    Message = "CulturalArticle deleted successfully."
                };
            }
            catch (Exception ex)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 500,
                    Message = "Error deleting article"
                };
            }
        }

        public async Task<ResponseDto> GetUserArticlesAsync(int userId)
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

            // Get all articles created by this user
            Expression<Func<CulturalArticle, bool>> articlePredicate = a => a.UserId == userId;


            string[] includes = { "Category", "Comments" };
            var userArticles = await _unitOfWork.Repository<CulturalArticle>().GetAllPredicated(articlePredicate, includes);


            if (!userArticles.Any())
            {
                return new ResponseDto
                {
                    IsSucceeded = true,
                    Status = 200,
                    Models = Enumerable.Empty<GetCulturalArticleDto>(),
                    Message = "No articles found for this user"
                };
            }


            var articleDtos = _mapper.Map<IEnumerable<GetCulturalArticleDto>>(userArticles);

            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Models = articleDtos,
                Message = "User articles retrieved successfully"
            };
        }

        public async Task<ArticleStatsDto> GetArticleStatsAsync(int articleId)
        {
            var (likeCount, commentCount) = await _articleRepository.GetArticleLikeAndCommentCountAsync(articleId);

            return new ArticleStatsDto
            {
                LikeCount = likeCount,
                CommentCount = commentCount
            };
        }
    }
}