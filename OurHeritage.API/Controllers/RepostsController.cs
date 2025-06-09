using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OurHeritage.API.Response;
using OurHeritage.Core.Specifications;
using OurHeritage.Service.DTOs.RepostDto;
using OurHeritage.Service.Interfaces;
using System.Security.Claims;

namespace OurHeritage.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RepostController : ControllerBase
    {
        private readonly IRepostService _repostService;

        public RepostController(IRepostService repostService)
        {
            _repostService = repostService;
        }

        [HttpPost("repost-cultural-article")]
        public async Task<IActionResult> AddRepostAsync([FromBody] AddRepostRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                request.UserId = userId;
            }
            else
            {
                return Unauthorized("Valid User ID is required");
            }
            var response = await _repostService.AddRepostAsync(request.UserId, request.CulturalArticleId, request.Content);
            if (!response.IsSucceeded)
            {
                return BadRequest(new ApiResponse(response.Status, response.Message));
            }
            return Ok(response.Model);
        }

        [HttpGet("get-reposts/{culturalArticleId}")]
        public async Task<ActionResult<PaginationResponse<GetRepostDto>>> GetRepostsByCulturalArticleAsync(
            int culturalArticleId,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10)
        {
            var paginatedResult = await _repostService.GetRepostsByCulturalArticleAsync(culturalArticleId, pageIndex, pageSize);
            return Ok(paginatedResult);
        }

        [HttpGet("count-reposts/{culturalArticleId}")]
        public async Task<IActionResult> CountRepostsAsync(int culturalArticleId)
        {
            var cntr = await _repostService.CountRepostsAsync(culturalArticleId);
            return Ok($"Total number of repost is: {cntr}");
        }

        [HttpGet("is-reposted/{userId}/{culturalArticleId}")]
        public async Task<IActionResult> IsRepostedAsync(int userId, int culturalArticleId)
        {
            var isReposted = await _repostService.IsRepostedAsync(userId, culturalArticleId);
            return Ok(isReposted);
        }

        [HttpDelete("remove/{repostId}")]
        public async Task<IActionResult> RemoveRepostAsync(int repostId)
        {
            var response = await _repostService.RemoveRepostAsync(repostId);
            if (!response.IsSucceeded)
            {
                return BadRequest(new ApiResponse(response.Status, response.Message));
            }
            return Ok(response.Message);
        }


    }
}