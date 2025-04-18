﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OurHeritage.API.Response;
using OurHeritage.Core.Entities;
using OurHeritage.Core.Specifications;
using OurHeritage.Repo.Repositories.Interfaces;
using OurHeritage.Service.DTOs;
using OurHeritage.Service.DTOs.CulturalArticleDto;
using OurHeritage.Service.Helper;
using OurHeritage.Service.Interfaces;
using System.Security.Claims;

namespace OurHeritage.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ArticlesController : ControllerBase
    {
        private readonly ICulturalArticleService _culturalArticleService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPaginationService _paginationService;

        public ArticlesController(ICulturalArticleService culturalArticleService, IUnitOfWork unitOfWork, IPaginationService paginationService)
        {
            _culturalArticleService = culturalArticleService;
            _unitOfWork = unitOfWork;
            _paginationService = paginationService;
        }

        [HttpGet]
        // [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PaginationResponse<GetCulturalArticleDto>>> GetArticles([FromQuery] SpecParams specParams)
        {
            var spec = new EntitySpecification<CulturalArticle>(specParams, e =>
                (string.IsNullOrEmpty(specParams.Search) || e.Content.ToLower().Contains(specParams.Search.ToLower())) &&
                (!specParams.FilterId.HasValue || e.CategoryId == specParams.FilterId));

            // Load Articles with related data (User, Category, Likes, Comments)
            var entities = await _unitOfWork.Repository<CulturalArticle>().GetAllPredicated(
                spec.Criteria, new[] { "User", "Category", "Likes", "Comments" });

            var totalEntities = await _unitOfWork.Repository<CulturalArticle>().CountAsync(spec);

            var response = _paginationService.Paginate<CulturalArticle, GetCulturalArticleDto>(
                entities, specParams, e => new GetCulturalArticleDto
                {
                    Id = e.Id,
                    Title = e.Title,
                    Content = e.Content,
                    CategoryId = e.CategoryId,
                    UserId = e.UserId,
                    ImageURL = e.ImageURL,
                    NameOfUser = e.User != null ? $"{e.User.FirstName} {e.User.LastName}" : "Unknown User",
                    UserProfilePicture = e.User?.ProfilePicture ?? "default.jpg",
                    NameOfCategory = e.Category.Name,
                    DateCreated = e.DateCreated.ToString("yyyy-MM-dd"), // Returns only the formatted date
                    TimeAgo = TimeAgoHelper.GetTimeAgo(e.DateCreated), // Returns time ago format
                    LikeCount = e.Likes?.Count ?? 0,
                    CommentCount = e.Comments?.Count ?? 0
                });

            return Ok(response);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetArticleById(int id)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var response = await _culturalArticleService.GetCulturalArticleByIdAsync(id, currentUserId);
            if (!response.IsSucceeded)
            {
                return NotFound(new ApiResponse(response.Status, response.Message));
            }
            return Ok(response.Model);
        }


        [HttpGet("feed")]
        public async Task<IActionResult> GetFeed()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var feed = await _culturalArticleService.GetUserFeedAsync(int.Parse(userId));

            if (feed.Models == null || !feed.Models.Any())
            {
                return NotFound(new ApiResponse(404, "No articles found in feed"));
            }

            return Ok(feed.Models);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchArticles([FromQuery] string content)
        {
            var response = await _culturalArticleService.GetCulturalArticlesByPredicateAsync(t => t.Content.Contains(content));
            if (response.Models == null || !response.Models.Any())
            {
                return NotFound(new ApiResponse(404, "No articles found matching the search criteria"));
            }
            return Ok(response.Models);
        }

        [HttpPost]
        public async Task<IActionResult> CreateArticle([FromForm] CreateOrUpdateCulturalArticleDto createDto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                createDto.UserId = userId;
            }
            else
            {
                return Unauthorized("Valid User ID is required");
            }
            var response = await _culturalArticleService.AddCulturalArticleAsync(createDto);
            if (!response.IsSucceeded)
            {
                return BadRequest(new ApiResponse(response.Status, response.Message));
            }
            return Ok(response.Model);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateArticle(int id, [FromForm] CreateOrUpdateCulturalArticleDto updateDto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                updateDto.UserId = userId;
            }
            else
            {
                return Unauthorized("Valid User ID is required");
            }
            var response = await _culturalArticleService.UpdateCulturalArticleAsync(id, updateDto);
            if (!response.IsSucceeded)
            {
                return BadRequest(new ApiResponse(response.Status, response.Message));
            }
            return Ok(response.Message);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteArticle(int id)
        {
            var response = await _culturalArticleService.DeleteCulturalArticleAsync(id);
            if (!response.IsSucceeded)
            {
                return BadRequest(new ApiResponse(response.Status, response.Message));
            }
            return Ok(response.Message);
        }

        [HttpGet("{id}/statistics")]
        public async Task<ActionResult<IEnumerable<ResponseDto>>> GetArticleStatistics(int id)
        {
            var response = await _culturalArticleService.GetCulturalArticleStatisticsAsync(id);
            if (!response.Success)
            {
                return NotFound(new ApiResponse(404, response.Message));
            }
            return Ok(response.Data);
        }

        [HttpGet("{userId}/articles")]
        public async Task<IActionResult> GetUserArticles(int userId)
        {
            var response = await _culturalArticleService.GetUserArticlesAsync(userId);

            if (!response.IsSucceeded)
            {
                return BadRequest(new ApiResponse(response.Status, response.Message));
            }

            return Ok(response);
        }

        // return count of likes & comments 
        [HttpGet("{id}/stats")]
        public async Task<IActionResult> GetArticleStats(int id)
        {
            var stats = await _culturalArticleService.GetArticleStatsAsync(id);

            return Ok(stats);
        }
    }
}
