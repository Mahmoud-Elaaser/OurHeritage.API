using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OurHeritage.API.Response;
using OurHeritage.Core.Entities;
using OurHeritage.Core.Specifications;
using OurHeritage.Repo.Repositories.Interfaces;
using OurHeritage.Service.DTOs.CommentDto;
using OurHeritage.Service.Interfaces;

namespace OurHeritage.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPaginationService _paginationService;

        public CommentsController(ICommentService commentService, IUnitOfWork unitOfWork, IPaginationService paginationService)
        {
            _commentService = commentService;
            _unitOfWork = unitOfWork;
            _paginationService = paginationService;
        }

        // Get all comments with pagination and filtering (Admin only)
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PaginationResponse<GetCommentDto>>> GetAllComments([FromQuery] SpecParams specParams)
        {
            var spec = new EntitySpecification<Comment>(specParams, e =>
                (string.IsNullOrEmpty(specParams.Search) || e.Content.ToLower().Contains(specParams.Search.ToLower())));

            var entities = await _unitOfWork.Repository<Comment>().ListAsync(spec);
            var response = _paginationService.Paginate<Comment, GetCommentDto>(entities, specParams, e => new GetCommentDto
            {
                Id = e.Id,
                Content = e.Content,
                CulturalArticleId = e.CulturalArticleId,
                UserId = e.UserId,
                DateCreated = e.DateCreated
            });

            return Ok(response);
        }

        // Get a comment by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCommentById(int id)
        {
            var response = await _commentService.GetCommentByIdAsync(id);
            if (!response.IsSucceeded)
            {
                return NotFound(new ApiResponse(response.Status, response.Message));
            }
            return Ok(response.Model);
        }

        // Add a new comment
        [HttpPost("add")]
        public async Task<IActionResult> AddComment([FromForm] CreateOrUpdateCommentDto dto)
        {
            var response = await _commentService.AddCommentAsync(dto);
            if (!response.IsSucceeded)
            {
                return BadRequest(new ApiResponse(response.Status, response.Message));
            }
            return Ok(response);
        }

        // Update an existing comment
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateComment(int id, [FromForm] CreateOrUpdateCommentDto dto)
        {
            var response = await _commentService.UpdateCommentAsync(id, dto);
            if (!response.IsSucceeded)
            {
                return BadRequest(new ApiResponse(response.Status, response.Message));
            }
            return Ok(response.Message);
        }

        // Delete a comment
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var response = await _commentService.DeleteCommentAsync(id);
            if (!response.IsSucceeded)
            {
                return BadRequest(new ApiResponse(response.Status, response.Message));
            }
            return Ok(response.Message);
        }

        // Get all comments on a cultural article with pagination
        [HttpGet("article/{culturalArticleId}")]
        public async Task<ActionResult<PaginationResponse<GetCommentDto>>> GetAllCommentsOnCulturalArticle(int culturalArticleId, [FromQuery] SpecParams specParams)
        {
            var spec = new EntitySpecification<Comment>(specParams, e => e.CulturalArticleId == culturalArticleId);

            var entities = await _unitOfWork.Repository<Comment>().ListAsync(spec);
            var response = _paginationService.Paginate<Comment, GetCommentDto>(entities, specParams, e => new GetCommentDto
            {
                Id = e.Id,
                Content = e.Content,
                CulturalArticleId = e.CulturalArticleId,
                UserId = e.UserId,
                DateCreated = e.DateCreated
            });

            return Ok(response);
        }
    }
}