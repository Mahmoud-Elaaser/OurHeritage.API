using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OurHeritage.API.Response;
using OurHeritage.Core.Entities;
using OurHeritage.Core.Specifications;
using OurHeritage.Repo.Repositories.Interfaces;
using OurHeritage.Service.DTOs;
using OurHeritage.Service.DTOs.CulturalArticleDto;
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
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PaginationResponse<CreateOrUpdateCulturalArticleDto>>> GetArticles([FromQuery] SpecParams specParams)
        {
            var spec = new EntitySpecification<CulturalArticle>(specParams, e =>
                (string.IsNullOrEmpty(specParams.Search) || e.Content.ToLower().Contains(specParams.Search.ToLower())) &&
                (!specParams.FilterId.HasValue || e.CategoryId == specParams.FilterId));

            var entities = await _unitOfWork.Repository<CulturalArticle>().ListAsync(spec);
            var totalEntities = await _unitOfWork.Repository<CulturalArticle>().CountAsync(spec);

            var response = _paginationService.Paginate<CulturalArticle, CreateOrUpdateCulturalArticleDto>(entities, specParams, e => new CreateOrUpdateCulturalArticleDto
            {
                Id = e.Id,
                Title = e.Title,
                Content = e.Content,
                CategoryId = e.CategoryId,
                DateCreated = e.DateCreated
            });

            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetArticleById(int id)
        {
            var response = await _culturalArticleService.GetCulturalArticleByIdAsync(id);
            if (response.Model == null)
            {
                return NotFound(new ApiResponse(404, "Article not found"));
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
        public async Task<IActionResult> CreateArticle([FromBody] CreateOrUpdateCulturalArticleDto createDto)
        {
            var response = await _culturalArticleService.AddCulturalArticleAsync(createDto);
            if (!response.IsSucceeded)
            {
                return BadRequest(new ApiResponse(response.Status, response.Message));
            }
            return Ok(response.Model);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateArticle(int id, [FromBody] CreateOrUpdateCulturalArticleDto updateDto)
        {
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
    }
}
