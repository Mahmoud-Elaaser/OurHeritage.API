using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OurHeritage.API.Response;
using OurHeritage.Service.DTOs;
using OurHeritage.Service.DTOs.CulturalArticleDto;
using OurHeritage.Service.Interfaces;
using System.Security.Claims;

namespace OurHeritage.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ArticlesController : ControllerBase
    {
        private readonly ICulturalArticleService _culturalArticleService;

        public ArticlesController(ICulturalArticleService culturalArticleService)
        {
            _culturalArticleService = culturalArticleService;
        }

        [HttpGet]
        //[Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<GetCulturalArticleDto>>> GetAllArticles()
        {
            var response = await _culturalArticleService.GetAllCulturalArticlesAsync();
            return Ok(response.Models);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetArticleById(int id)
        {
            var response = await _culturalArticleService.GetCulturalArticleByIdAsync(id);
            return Ok(response.Model);
        }


        /// Retrieve all articles except articles from user you muted
        [HttpGet("feed")]
        public async Task<IActionResult> GetFeed()
        {
            /// get user from claims
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var feed = await _culturalArticleService.GetUserFeedAsync(int.Parse(userId));

            return Ok(feed.Models);
        }


        [HttpGet("search-by-content")]
        public async Task<IActionResult> GetArticlesByContent([FromQuery] string content)
        {
            var response = await _culturalArticleService.GetCulturalArticlesByPredicateAsync(t => t.Content.Contains(content));
            return Ok(response.Models);
        }

        [HttpPost("create-cultural-article")]
        public async Task<IActionResult> CreateArticle([FromBody] CreateOrUpdateCulturalArticleDto createArticleDto)
        {
            var response = await _culturalArticleService.AddCulturalArticleAsync(createArticleDto);
            if (!response.IsSucceeded)
            {
                return BadRequest(new ApiResponse(response.Status, response.Message));
            }
            return Ok(response.Model);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateArticle(int id, [FromBody] CreateOrUpdateCulturalArticleDto updateTweetDto)
        {
            var response = await _culturalArticleService.UpdateCulturalArticleAsync(id, updateTweetDto);
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

        [HttpGet("{articleId}/statistics")]
        public async Task<ActionResult<IEnumerable<ResponseDto>>> GetArticleStatistics(int articleId)
        {
            var response = await _culturalArticleService.GetCulturalArticleStatisticsAsync(articleId);

            if (!response.Success)
            {
                return BadRequest(new ApiResponse(404, response.Message));
            }

            return Ok(response.Data);
        }

    }
}
