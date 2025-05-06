using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OurHeritage.API.Response;
using OurHeritage.Core.Entities;
using OurHeritage.Core.Specifications;
using OurHeritage.Repo.Repositories.Interfaces;
using OurHeritage.Service.DTOs.CommentDto;
using OurHeritage.Service.Interfaces;
using System.Security.Claims;

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

        [HttpGet]
        // [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PaginationResponse<GetCommentDto>>> GetAllComments([FromQuery] SpecParams specParams)
        {
            var spec = new EntitySpecification<Comment>(specParams, e =>
                (string.IsNullOrEmpty(specParams.Search) || e.Content.ToLower().Contains(specParams.Search.ToLower())));

            // Include User entity to fetch creator details
            var entities = await _unitOfWork.Repository<Comment>()
                .GetAllPredicated(spec.Criteria, new[] { "User" });
            var response = _paginationService.Paginate(entities, specParams, e => new GetCommentDto
            {
                Id = e.Id,
                Content = e.Content,
                CulturalArticleId = e.CulturalArticleId,
                UserId = e.UserId,
                NameOfUser = e.User != null ? $"{e.User.FirstName} {e.User.LastName}" : "Unknown User",
                UserProfilePicture = e.User?.ProfilePicture ?? "default.jpg",
                DateCreated = e.DateCreated
            });

            return Ok(response);
        }

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

        [HttpPost("add")]
        public async Task<IActionResult> AddComment([FromForm] CreateOrUpdateCommentDto dto)
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
            var response = await _commentService.AddCommentAsync(dto);
            if (!response.IsSucceeded)
            {
                return BadRequest(new ApiResponse(response.Status, response.Message));
            }
            return Ok(response.Message);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateComment(int id, [FromForm] CreateOrUpdateCommentDto dto)
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
            var response = await _commentService.UpdateCommentAsync(id, dto);
            if (!response.IsSucceeded)
            {
                return BadRequest(new ApiResponse(response.Status, response.Message));
            }
            return Ok(response.Message);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var response = await _commentService.DeleteCommentAsync(User, id);
            if (!response.IsSucceeded)
            {
                return BadRequest(new ApiResponse(response.Status, response.Message));
            }
            return Ok(response.Message);
        }

        [HttpGet("article/{culturalArticleId}")]
        public async Task<ActionResult<PaginationResponse<GetCommentDto>>> GetAllCommentsOnCulturalArticle(int culturalArticleId, [FromQuery] SpecParams specParams)
        {
            var spec = new EntitySpecification<Comment>(specParams, e => e.CulturalArticleId == culturalArticleId);

            // Include User entity to fetch creator details
            var entities = await _unitOfWork.Repository<Comment>()
                .GetAllPredicated(spec.Criteria, new[] { "User" });
            var response = _paginationService.Paginate(entities, specParams, e => new GetCommentDto
            {
                Id = e.Id,
                Content = e.Content,
                CulturalArticleId = e.CulturalArticleId,
                UserId = e.UserId,
                NameOfUser = e.User != null ? $"{e.User.FirstName} {e.User.LastName}" : "Unknown User",
                UserProfilePicture = e.User?.ProfilePicture ?? "default.jpg",
                DateCreated = e.DateCreated
            });

            return Ok(response);
        }
    }
}