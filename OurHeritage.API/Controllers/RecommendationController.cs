using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OurHeritage.Core.Enums;
using OurHeritage.Service.DTOs;
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
        private readonly ILogger<RecommendationController> _logger;

        public RecommendationController(
            IRecommendationService recommendationService,
            ILogger<RecommendationController> logger)
        {
            _recommendationService = recommendationService;
            _logger = logger;
        }


        [HttpGet]
        public async Task<ActionResult<List<RecommendationResult>>> GetRecommendations([FromQuery] int count = 10)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized("User not authenticated");

                var recommendations = await _recommendationService.GetRecommendationsAsync(userId.Value, RecommendationType.Mixed, count);
                return Ok(recommendations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting mixed recommendations");
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpGet("by-type")]
        public async Task<ActionResult<List<RecommendationResult>>> GetRecommendationsByType(
            [FromQuery] RecommendationType type = RecommendationType.Mixed,
            [FromQuery] int count = 10)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized("User not authenticated");

                var recommendations = await _recommendationService.GetRecommendationsAsync(userId.Value, type, count);
                return Ok(recommendations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recommendations by type {Type}", type);
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpGet("cultural")]
        public async Task<ActionResult<List<RecommendationResult>>> GetCulturalRecommendations([FromQuery] int count = 10)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized("User not authenticated");

                var recommendations = await _recommendationService.GetCulturalRecommendationsAsync(userId.Value, count);
                return Ok(recommendations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cultural recommendations");
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpGet("handicrafts")]
        public async Task<ActionResult<List<RecommendationResult>>> GetHandicraftRecommendations([FromQuery] int count = 10)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized("User not authenticated");

                var recommendations = await _recommendationService.GetHandicraftRecommendationsAsync(userId.Value, count);
                return Ok(recommendations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting handicraft recommendations");
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpGet("handicrafts/{handicraftId}/similar")]
        public async Task<ActionResult<List<RecommendationResult>>> GetSimilarHandicrafts(
            int handicraftId,
            [FromQuery] int count = 5)
        {
            try
            {
                if (handicraftId <= 0)
                    return BadRequest("Invalid handicraft ID");

                var recommendations = await _recommendationService.GetSimilarHandicraftsAsync(handicraftId, count);
                return Ok(recommendations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting similar handicrafts for ID {HandicraftId}", handicraftId);
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpGet("cultural/{culturalId}/similar")]
        public async Task<ActionResult<List<RecommendationResult>>> GetSimilarCulturals(
            int culturalId,
            [FromQuery] int count = 5)
        {
            try
            {
                if (culturalId <= 0)
                    return BadRequest("Invalid cultural ID");

                var recommendations = await _recommendationService.GetSimilarCulturalsAsync(culturalId, count);
                return Ok(recommendations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting similar culturals for ID {CulturalId}", culturalId);
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpGet("handicrafts/trending")]
        [AllowAnonymous]
        public async Task<ActionResult<List<RecommendationResult>>> GetTrendingHandicrafts([FromQuery] int count = 10)
        {
            try
            {
                var recommendations = await _recommendationService.GetTrendingHandicraftsAsync(count);
                return Ok(recommendations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting trending handicrafts");
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpGet("handicrafts/recent")]
        [AllowAnonymous]
        public async Task<ActionResult<List<RecommendationResult>>> GetRecentHandicrafts([FromQuery] int count = 10)
        {
            try
            {
                var recommendations = await _recommendationService.GetRecentHandicraftsAsync(count);
                return Ok(recommendations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent handicrafts");
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpGet("handicrafts/price-range")]
        public async Task<ActionResult<List<RecommendationResult>>> GetHandicraftsByPriceRange(
            [FromQuery] double minPrice,
            [FromQuery] double maxPrice,
            [FromQuery] int count = 10)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized("User not authenticated");

                if (minPrice < 0 || maxPrice < 0 || minPrice > maxPrice)
                    return BadRequest("Invalid price range");

                var recommendations = await _recommendationService.GetHandicraftsByPriceRangeAsync(userId.Value, minPrice, maxPrice, count);
                return Ok(recommendations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting handicrafts by price range {MinPrice}-{MaxPrice}", minPrice, maxPrice);
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpPost("refresh-preferences")]
        public async Task<ActionResult> RefreshUserPreferences()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized("User not authenticated");

                await _recommendationService.UpdateUserPreferencesAsync(userId.Value);
                return Ok(new { message = "User preferences updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing user preferences");
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpGet("stats")]
        public async Task<ActionResult<object>> GetRecommendationStats()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized("User not authenticated");

                // Get different types of recommendations to build stats
                var culturalRecs = await _recommendationService.GetCulturalRecommendationsAsync(userId.Value, 5);
                var handicraftRecs = await _recommendationService.GetHandicraftRecommendationsAsync(userId.Value, 5);

                var stats = new
                {
                    UserId = userId.Value,
                    CulturalRecommendationsAvailable = culturalRecs.Count,
                    HandicraftRecommendationsAvailable = handicraftRecs.Count,
                    TopCulturalRecommendation = culturalRecs.FirstOrDefault()?.Item?.Title,
                    TopHandicraftRecommendation = handicraftRecs.FirstOrDefault()?.Item?.Title,
                    LastUpdated = DateTime.UtcNow
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recommendation stats");
                return StatusCode(500, "Internal server error");
            }
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }
            return null;
        }
    }
}
