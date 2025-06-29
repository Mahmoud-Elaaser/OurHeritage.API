﻿using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OurHeritage.Core.Context;
using OurHeritage.Core.Entities;
using OurHeritage.Repo.Repositories.Interfaces;
using OurHeritage.Service.DTOs;
using OurHeritage.Service.DTOs.FavoriteDto;
using OurHeritage.Service.DTOs.HandiCraftDto;
using OurHeritage.Service.Helper;
using OurHeritage.Service.Interfaces;
using System.Security.Claims;

namespace OurHeritage.Service.Implementations
{
    public class HandiCraftService : IHandiCraftService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;

        public HandiCraftService(IUnitOfWork unitOfWork, IMapper mapper, ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _context = context;
        }

        public async Task<ResponseDto> CreateHandiCraftAsync(CreateOrUpdateHandiCraftDto dto)
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
            foreach (var image in dto.Images)
            {
                var uploadedFiles = FilesSetting.UploadFile(image, "HandiCraft");
                if (uploadedFiles != null && uploadedFiles.Any())
                {
                    dto.ImageOrVideo.Add(uploadedFiles);
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
            var HandiCraft = _mapper.Map<HandiCraft>(dto);
            await _unitOfWork.Repository<HandiCraft>().AddAsync(HandiCraft);
            await _unitOfWork.CompleteAsync();
            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 201,
                Model = dto,
                Message = "HandiCraft created successfully"
            };
        }

        public async Task<ResponseDto> GetHandiCraftByIdAsync(int id)
        {
            var handiCraft = (await _unitOfWork.Repository<HandiCraft>()
                .GetAllPredicated(h => h.Id == id, new[] { "User", "Category", "Favorite" }))
                .FirstOrDefault();

            if (handiCraft == null)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 404,
                    Message = "HandiCraft not found"
                };
            }

            var handiCraftDto = new GetHandiCraftDto
            {
                Id = handiCraft.Id,
                Title = handiCraft.Title,
                Description = handiCraft.Description,
                ImageOrVideo = handiCraft.ImageOrVideo,
                Price = handiCraft.Price,
                CategoryId = handiCraft.CategoryId,
                CategoryName = handiCraft.Category?.Name ?? "Unknown Category",
                UserId = handiCraft.UserId,
                NameOfUser = handiCraft.User != null ? $"{handiCraft.User.FirstName} {handiCraft.User.LastName}" : "Unknown User",
                UserProfilePicture = handiCraft.User?.ProfilePicture ?? "default.jpg",
                DateAdded = handiCraft.DateAdded.ToString("yyyy-MM-dd"),
                FavoriteCount = handiCraft.Favorite?.Count ?? 0,
                TimeAgo = TimeAgoHelper.GetTimeAgo(handiCraft.DateAdded),
                FavoritedBy = handiCraft.Favorite?.Select(f => new GetFavoriteDto  // List of users who favorited it
                {
                    UserId = f.UserId,
                    CreatorName = f.User != null ? $"{f.User.FirstName} {f.User.LastName}" : "Unknown User",
                    CreatorProfilePicture = f.User?.ProfilePicture ?? "default.jpg",
                    DateCreated = f.DateCreated.ToString("yyyy-MM-dd"),
                    HandiCraftId = f.HandiCraftId,
                    HandiCraftTitle = f.HandiCraft.Title,
                    Id = f.Id

                }).ToList() ?? new List<GetFavoriteDto>()
            };

            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Model = handiCraftDto
            };
        }


        public async Task<ResponseDto> GetAllHandiCraftsAsync()
        {
            var handiCrafts = await _unitOfWork.Repository<HandiCraft>()
                .GetAllPredicated(h => true, new[] { "User", "Category", "Favorite" });

            var mappedHandiCrafts = handiCrafts.Select(h => new GetHandiCraftDto
            {
                Id = h.Id,
                Title = h.Title,
                Description = h.Description,
                ImageOrVideo = h.ImageOrVideo,
                Price = h.Price,
                CategoryId = h.CategoryId,
                CategoryName = h.Category?.Name ?? "Unknown Category",
                UserId = h.UserId,
                NameOfUser = h.User != null ? $"{h.User.FirstName} {h.User.LastName}" : "Unknown User",
                UserProfilePicture = h.User?.ProfilePicture ?? "default.jpg",
                DateAdded = h.DateAdded.ToString("yyyy-MM-dd"),
                FavoriteCount = h.Favorite?.Count ?? 0,
                TimeAgo = TimeAgoHelper.GetTimeAgo(h.DateAdded),
                FavoritedBy = h.Favorite != null && h.Favorite.Count > 0
                    ? h.Favorite.Select(f => new GetFavoriteDto
                    {
                        UserId = f.UserId,
                        CreatorName = f.User != null ? $"{f.User.FirstName} {f.User.LastName}" : "Unknown User",
                        CreatorProfilePicture = f.User?.ProfilePicture ?? "default.jpg",
                        DateCreated = f.DateCreated.ToString("yyyy-MM-dd"),
                        HandiCraftId = f.HandiCraftId,
                        Id = f.Id,
                        HandiCraftTitle = f.HandiCraft.Title
                    }).ToList()
                    : new List<GetFavoriteDto>()

            }).ToList();

            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Models = mappedHandiCrafts
            };
        }

        public async Task<ResponseDto> UpdateHandiCraftAsync(int HandiCraftId, CreateOrUpdateHandiCraftDto dto)
        {

            var HandiCraft = await _unitOfWork.Repository<HandiCraft>().GetByIdAsync(HandiCraftId);
            if (HandiCraft == null)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 404,
                    Message = "HandiCraft not found"
                };
            }
            if (dto.Images != null)
            {
                foreach (var imageUrl in dto.ImageOrVideo)
                {
                    FilesSetting.DeleteFile(imageUrl);

                }
                foreach (var image in dto.Images)
                {
                    var uploadedFiles = FilesSetting.UploadFile(image, "CulturalArticle");
                    if (uploadedFiles != null && uploadedFiles.Any())
                    {
                        dto.ImageOrVideo.Add(uploadedFiles);
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

            _mapper.Map(dto, HandiCraft);
            _unitOfWork.Repository<HandiCraft>().Update(HandiCraft);
            await _unitOfWork.CompleteAsync();
            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Message = "HandiCraft updated successfully"
            };
        }

        public async Task<ResponseDto> DeleteHandiCraftAsync(ClaimsPrincipal user, int handiCraftId)
        {
            try
            {
                var handiCraft = await _context.HandiCrafts
                    .Include(h => h.Favorite)
                    .FirstOrDefaultAsync(h => h.Id == handiCraftId);

                if (handiCraft == null)
                {
                    return new ResponseDto
                    {
                        IsSucceeded = false,
                        Status = 404,
                        Message = "HandiCraft not found - must have been too crafty!"
                    };
                }

                // Check if user is the crafty creator or the mighty admin
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                var userRole = user.FindFirstValue(ClaimTypes.Role);

                if (userId != handiCraft.UserId.ToString() && userRole != "Admin")
                {
                    return new ResponseDto
                    {
                        IsSucceeded = false,
                        Status = 403,
                        Message = "Not your craft to dismantle!"
                    };
                }

                if (handiCraft.Favorite?.Any() == true)
                {
                    _context.Favorites.RemoveRange(handiCraft.Favorite);
                }


                _unitOfWork.Repository<HandiCraft>().Delete(handiCraft);
                await _unitOfWork.CompleteAsync();

                return new ResponseDto
                {
                    IsSucceeded = true,
                    Status = 200,
                    Message = "HandiCraft deleted successfully!"
                };
            }
            catch (Exception ex)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 500,
                    Message = $"Craft deletion failed: {ex.Message}"
                };
            }
        }


    }
}