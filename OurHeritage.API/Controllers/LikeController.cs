using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OurHeritage.Service.DTOs;
using OurHeritage.Service.DTOs.LikeDto;
using OurHeritage.Service.Interfaces;

namespace OurHeritage.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LikeController : ControllerBase
    {
        private readonly ILikeService _likeService;

        public LikeController(ILikeService likeService)
        {
            _likeService = likeService;
        }

        [HttpPost("add-like")]
        public async Task<IActionResult> AddLike(CreateLikeDto dto)
        {
            var result = await _likeService.AddLikeAsync(dto);
            if (!result.IsSucceeded)
            {
                return BadRequest(
                    new ResponseDto
                    {
                        Message = "Could not add like. The user might be blocked or the like already exists.",
                        IsSucceeded = false,
                        Status = result.Status
                    }
                );
            }

            return Ok(result.Message);
        }

        [HttpDelete("remove-like")]
        public async Task<IActionResult> RemoveLike(int culturalArticleId, int userId)
        {
            var result = await _likeService.RemoveLikeAsync(culturalArticleId, userId);
            if (!result.IsSucceeded)
            {
                return BadRequest(
                    new ResponseDto
                    {
                        Message = "Couldn't remove like. It may not exist.",
                        IsSucceeded = false,
                        Status = result.Status
                    }
                );
            }

            return Ok(result.Message);
        }

        [HttpGet("count-likes")]
        public async Task<IActionResult> CountLikes(int culturalArticleId)
        {
            var count = await _likeService.CountLikesAsync(culturalArticleId);
            return Ok(new { TweetId = culturalArticleId, LikesCount = count });
        }

        [HttpGet("isLiked")]
        public async Task<IActionResult> IsLiked(int culturalArticleId, int userId)
        {
            var isLiked = await _likeService.IsLikedAsync(culturalArticleId, userId);
            return Ok(new { TweetId = culturalArticleId, IsLiked = isLiked });
        }

        [HttpGet("get-who-liked-an-article")]
        public async Task<IActionResult> GetUsersWhoLikedArticle(int culturalArticleId)
        {
            var users = await _likeService.GetUsersWhoLikedArticleAsync(culturalArticleId);
            if (users == null || !users.Any())
            {
                return BadRequest(
                    new ResponseDto
                    {
                        Message = "No users liked this article",
                        IsSucceeded = false,
                        Status = 404
                    }
                );
            }

            return Ok(users);
        }

        [HttpGet("liked-articles-by-user")]
        public async Task<IActionResult> GetLikedarticles(int userId)
        {
            var likedTweets = await _likeService.GetLikedArticlesAsync(userId);
            if (likedTweets == null || !likedTweets.Any())
            {
                return BadRequest(
                    new ResponseDto
                    {
                        Message = "No liked articles found.",
                        IsSucceeded = false,
                        Status = 404
                    }
                );
            }

            return Ok(likedTweets);
        }

        [HttpGet("all-likes-on-an-article")]
        public async Task<ActionResult<IEnumerable<GetLikeDto>>> GetAllLikesOnArticle(int culturalArticleId)
        {
            var response = await _likeService.GetAllLikesOnCulturalArticleAsync(culturalArticleId);
            return Ok(response.Models);
        }

    }
}
