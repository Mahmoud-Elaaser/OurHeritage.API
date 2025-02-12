using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OurHeritage.API.Response;
using OurHeritage.Service.DTOs.CommentDto;
using OurHeritage.Service.Interfaces;

namespace OurHeritage.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentsController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpGet]
        //[Authorize(Roles = "Admin")]
        public async Task<ActionResult<GetCommentDto>> GetAllComments()
        {
            var response = await _commentService.GetAllCommentsAsync();
            return Ok(response.Models);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCommentById(int id)
        {
            var response = await _commentService.GetCommentByIdAsync(id);
            if (!response.IsSucceeded)
            {
                return BadRequest(new ApiResponse(response.Status, response.Message));
            }
            return Ok(response.Model);
        }

        [HttpPost("add-comment")]
        public async Task<IActionResult> AddComment([FromBody] CreateOrUpdateCommentDto dto)
        {
            var response = await _commentService.AddCommentAsync(dto);
            if (!response.IsSucceeded)
            {
                return BadRequest(new ApiResponse(response.Status, response.Message));
            }
            return Ok(response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateComment(int id, [FromBody] CreateOrUpdateCommentDto dto)
        {
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
            var response = await _commentService.DeleteCommentAsync(id);
            if (!response.IsSucceeded)
            {
                return BadRequest(new ApiResponse(response.Status, response.Message));
            }
            return Ok(response.Message);
        }

        [HttpGet("get-all-comments-on-a-cultural-article")]
        public async Task<ActionResult<IEnumerable<GetCommentDto>>> GetAllCommentsOnTweet(int culturalArticleId)
        {
            var response = await _commentService.GetAllCommentsOnCulturalArticleAsync(culturalArticleId);
            return Ok(response.Models);
        }
    }
}
