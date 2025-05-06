using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OurHeritage.Core.Entities;
using OurHeritage.Core.Specifications;
using OurHeritage.Repo.Repositories.Interfaces;
using OurHeritage.Service.DTOs;
using OurHeritage.Service.DTOs.LikeDto;
using OurHeritage.Service.Interfaces;
using System.Security.Claims;

namespace OurHeritage.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LikeController : ControllerBase
    {
        private readonly ILikeService _likeService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPaginationService _paginationService;

        public LikeController(ILikeService likeService, IUnitOfWork unitOfWork, IPaginationService paginationService)
        {
            _likeService = likeService;
            _unitOfWork = unitOfWork;
            _paginationService = paginationService;
        }

        [HttpPost("add-like")]
        public async Task<IActionResult> AddLike([FromBody] CreateLikeDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                dto.UserId = userId;
            }
            else
            {
                return Unauthorized("Valid User ID is required");
            }
            var result = await _likeService.AddLikeAsync(dto);
            if (!result.IsSucceeded)
            {
                return BadRequest(new ResponseDto
                {
                    Message = result.Message,
                    IsSucceeded = false,
                    Status = result.Status
                });
            }
            return Ok(result.Message);
        }

        [HttpDelete("remove-like")]
        public async Task<IActionResult> RemoveLike([FromQuery] int culturalArticleId, [FromQuery] int userId)
        {
            var result = await _likeService.RemoveLikeAsync(culturalArticleId, userId);
            if (!result.IsSucceeded)
            {
                return BadRequest(new ResponseDto
                {
                    Message = result.Message,
                    IsSucceeded = false,
                    Status = result.Status
                });
            }
            return Ok(new { Message = result.Message });
        }

        [HttpGet("count-likes")]
        public async Task<IActionResult> CountLikes([FromQuery] int culturalArticleId)
        {
            var count = await _likeService.CountLikesAsync(culturalArticleId);
            return Ok(new { CulturalArticleId = culturalArticleId, LikesCount = count });
        }

        [HttpGet("is-liked")]
        public async Task<IActionResult> IsLiked([FromQuery] int culturalArticleId, [FromQuery] int userId)
        {
            var isLiked = await _likeService.IsLikedAsync(culturalArticleId, userId);
            return Ok(new { CulturalArticleId = culturalArticleId, IsLiked = isLiked });
        }

        [HttpGet("get-users-who-liked-article")]
        public async Task<IActionResult> GetUsersWhoLikedArticle([FromQuery] int culturalArticleId)
        {
            var users = await _likeService.GetUsersWhoLikedArticleAsync(culturalArticleId);

            if (users == null || !users.Any())
            {
                return NotFound(new ResponseDto
                {
                    Message = "No users liked this article.",
                    IsSucceeded = false,
                    Status = 404
                });
            }
            return Ok(users);
        }

        [HttpGet("liked-articles-by-user")]
        public async Task<IActionResult> GetLikedArticles([FromQuery] int userId)
        {
            var likedArticles = await _likeService.GetLikedArticlesAsync(userId);
            if (likedArticles == null || !likedArticles.Any())
            {
                return NotFound(new ResponseDto
                {
                    Message = "No liked articles found.",
                    IsSucceeded = false,
                    Status = 404
                });
            }
            return Ok(likedArticles);
        }

        [HttpGet("all-likes-on-article")]
        public async Task<ActionResult<PaginationResponse<GetLikeDto>>> GetAllLikesOnArticle([FromQuery] int culturalArticleId, [FromQuery] SpecParams specParams)
        {
            var spec = new EntitySpecification<Like>(specParams, e => e.CulturalArticleId == culturalArticleId);
            // Include User entity to fetch creator details
            var entities = await _unitOfWork.Repository<Like>()
                .GetAllPredicated(spec.Criteria, new[] { "User" });
            var response = _paginationService.Paginate(entities, specParams, e => new GetLikeDto
            {
                Id = e.Id,
                CulturalArticleId = e.CulturalArticleId,
                UserId = e.UserId,
                NameOfUser = e.User != null ? $"{e.User.FirstName} {e.User.LastName}" : "Unknown User",
                UserProfilePicture = e.User?.ProfilePicture ?? "default.jpg",
                LikedAt = e.LikedAt
            });

            return Ok(response);
        }
    }
}