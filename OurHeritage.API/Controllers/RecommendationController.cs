using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OurHeritage.Core.Enums;
using OurHeritage.Service.DTOs.RecommendationSystemDto;
using OurHeritage.Service.Interfaces;
using System.Security.Claims;

namespace OurHeritage.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RecommendationController : ControllerBase
    {
        private readonly IRecommendationService _recommendationService;

        public RecommendationController(IRecommendationService recommendationService)
        {
            _recommendationService = recommendationService;
        }

        [HttpGet("for-user")]
        public async Task<ActionResult<RecommendationResponseDto>> GetRecommendationsForUser(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] RecommendationType filterType = RecommendationType.Both)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized("User not found");

            var recommendations = await _recommendationService.GetRecommendationsAsync(
                userId, pageSize, pageNumber, filterType);

            return Ok(recommendations);
        }

        [HttpGet("by-category/{categoryId}")]
        public async Task<ActionResult<RecommendationResponseDto>> GetRecommendationsByCategory(
            int categoryId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] RecommendationType filterType = RecommendationType.Both)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized("User not found");

            var recommendations = await _recommendationService.GetRecommendationsByCategoryAsync(
                userId, categoryId, filterType, pageSize, pageNumber);

            return Ok(recommendations);
        }


        [HttpGet("similar-articles/{articleId}")]
        public async Task<ActionResult<List<RecommendationDto>>> GetSimilarArticles(
            int articleId,
            [FromQuery] int count = 5)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized("User not found");

            var recommendations = await _recommendationService.GetSimilarArticlesAsync(articleId, userId, count);
            return Ok(recommendations);
        }


        [HttpGet("similar-handicrafts/{handicraftId}")]
        public async Task<ActionResult<List<RecommendationDto>>> GetSimilarHandicrafts(
            int handicraftId,
            [FromQuery] int count = 5)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized("User not found");

            var recommendations = await _recommendationService.GetSimilarHandicraftsAsync(handicraftId, userId, count);
            return Ok(recommendations);
        }


        [HttpGet("user-engagement")]
        public async Task<ActionResult<UserEngagementDto>> GetUserEngagement()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized("User not found");

            var engagement = await _recommendationService.GetUserEngagementDataAsync(userId);
            return Ok(engagement);
        }





        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }
    }


    public class UpdateEngagementRequest
    {
        public string EngagementType { get; set; } = string.Empty; // "like", "comment", "favorite"
        public int ItemId { get; set; }
    }

    public class AdvancedRecommendationRequest
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public RecommendationType FilterType { get; set; } = RecommendationType.Both;
        public int? CategoryId { get; set; }
    }
}