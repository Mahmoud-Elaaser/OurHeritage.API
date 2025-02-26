using AutoMapper;
using Microsoft.AspNetCore.Http;
using OurHeritage.Core.Entities;
using OurHeritage.Core.Specifications;
using OurHeritage.Repo.Repositories.Interfaces;
using OurHeritage.Service.DTOs;
using OurHeritage.Service.DTOs.CulturalArticleDto;
using OurHeritage.Service.Helper;
using OurHeritage.Service.Interfaces;
using System.Collections.Generic;
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

        public async Task<ResponseDto> GetCulturalArticleByIdAsync(int id)
        {
            var culturalArticle = await _unitOfWork.Repository<CulturalArticle>().GetByIdAsync(id);
            if (culturalArticle == null)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 404,
                    Message = "CulturalArticle not found"
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
            foreach (var image in createCulturalArticleDto.Images)
            {
                var uploadedFiles = FilesSetting.UploadFile(image ,"CulturalArticle");
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
                    FilesSetting.DeleteFile(imageUrl, "CulturalArticle");

                }
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
            var culturalArticle = await _unitOfWork.Repository<CulturalArticle>().GetByIdAsync(id);
            if (culturalArticle == null)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 404,
                    Message = "CulturalArticle not found"
                };
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
    }
}