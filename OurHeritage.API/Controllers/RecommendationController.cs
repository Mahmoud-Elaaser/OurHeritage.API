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

        /// <summary>
        /// Get mixed recommendations (both cultural and handicrafts)
        /// </summary>
        /// <param name="count">Number of recommendations to return (default: 10)</param>
        /// <returns>List of mixed recommendations</returns>
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

        /// <summary>
        /// Get recommendations by specific type
        /// </summary>
        /// <param name="type">Type of recommendations (Cultural, Handicraft, Mixed)</param>
        /// <param name="count">Number of recommendations to return (default: 10)</param>
        /// <returns>List of recommendations by type</returns>
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

        /// <summary>
        /// Get cultural recommendations only
        /// </summary>
        /// <param name="count">Number of recommendations to return (default: 10)</param>
        /// <returns>List of cultural recommendations</returns>
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

        /// <summary>
        /// Get handicraft recommendations only
        /// </summary>
        /// <param name="count">Number of recommendations to return (default: 10)</param>
        /// <returns>List of handicraft recommendations</returns>
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

        /// <summary>
        /// Get similar handicrafts based on a specific handicraft
        /// </summary>
        /// <param name="handicraftId">ID of the handicraft to find similar items for</param>
        /// <param name="count">Number of similar items to return (default: 5)</param>
        /// <returns>List of similar handicrafts</returns>
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

        /// <summary>
        /// Get similar cultural items based on a specific cultural item
        /// </summary>
        /// <param name="culturalId">ID of the cultural item to find similar items for</param>
        /// <param name="count">Number of similar items to return (default: 5)</param>
        /// <returns>List of similar cultural items</returns>
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

        /// <summary>
        /// Get trending handicrafts
        /// </summary>
        /// <param name="count">Number of trending items to return (default: 10)</param>
        /// <returns>List of trending handicrafts</returns>
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

        /// <summary>
        /// Get recently added handicrafts
        /// </summary>
        /// <param name="count">Number of recent items to return (default: 10)</param>
        /// <returns>List of recently added handicrafts</returns>
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

        /// <summary>
        /// Get handicrafts within a specific price range
        /// </summary>
        /// <param name="minPrice">Minimum price</param>
        /// <param name="maxPrice">Maximum price</param>
        /// <param name="count">Number of items to return (default: 10)</param>
        /// <returns>List of handicrafts within price range</returns>
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

        /// <summary>
        /// Update user preferences (refresh recommendation cache)
        /// </summary>
        /// <returns>Success message</returns>
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

        /// <summary>
        /// Get user's recommendation statistics
        /// </summary>
        /// <returns>User recommendation statistics</returns>
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

        /// <summary>
        /// Get recommendation results as separated lists
        /// </summary>
        /// <param name="type">Type of recommendations</param>
        /// <param name="count">Number of recommendations</param>
        /// <returns>Separated cultural and handicraft lists</returns>
        //[HttpGet("separated")]
        //public async Task<ActionResult<object>> GetSeparatedRecommendations(
        //    [FromQuery] RecommendationType type = RecommendationType.Mixed,
        //    [FromQuery] int count = 10)
        //{
        //    try
        //    {
        //        var userId = GetCurrentUserId();
        //        if (userId == null)
        //            return Unauthorized("User not authenticated");

        //        var recommendations = await _recommendationService.GetRecommendationsAsync(userId.Value, type, count);

        //        var response = new
        //        {
        //            CulturalItems = recommendations.GetCulturalRecommendations(),
        //            HandicraftItems = recommendations.GetHandicraftRecommendations(),
        //            TotalCount = recommendations.Count
        //        };

        //        return Ok(response);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error getting separated recommendations");
        //        return StatusCode(500, "Internal server error");
        //    }
        //}

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
