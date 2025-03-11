using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OurHeritage.API.Response;
using OurHeritage.Core.Entities;
using OurHeritage.Core.Specifications;
using OurHeritage.Repo.Repositories.Interfaces;
using OurHeritage.Service.DTOs.FavoriteDto;
using OurHeritage.Service.Interfaces;
using System.Security.Claims;

namespace OurHeritage.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FavoritesController : ControllerBase
    {
        private readonly IFavoriteService _favoriteService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPaginationService _paginationService;

        public FavoritesController(IFavoriteService favoriteService, IUnitOfWork unitOfWork, IPaginationService paginationService)
        {
            _favoriteService = favoriteService;
            _unitOfWork = unitOfWork;
            _paginationService = paginationService;
        }

        [HttpGet("handicraft/{handicraftId}")]
        public async Task<ActionResult<PaginationResponse<GetFavoriteDto>>> GetAllFavoritesOnHandicraft(int handicraftId, [FromQuery] SpecParams specParams)
        {
            var spec = new EntitySpecification<Favorite>(specParams, e => e.HandiCraftId == handicraftId);

            var entities = await _unitOfWork.Repository<Favorite>()
                .GetAllPredicated(spec.Criteria, new[] { "User", "HandiCraft" });

            var response = _paginationService.Paginate(entities, specParams, e => new GetFavoriteDto
            {
                Id = e.Id,
                UserId = e.UserId,
                HandiCraftId = e.HandiCraftId,
                HandiCraftTitle = e.HandiCraft?.Title ?? "Unknown",
                DateCreated = e.DateCreated,
                CreatorName = e.User != null ? $"{e.User.FirstName} {e.User.LastName}" : "Unknown User",
                CreatorProfilePicture = e.User?.ProfilePicture ?? "default.jpg",
            });

            return Ok(response);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetFavoriteById(int id)
        {
            var response = await _favoriteService.GetFavoriteByIdAsync(id);
            if (!response.IsSucceeded)
                return NotFound(new ApiResponse(response.Status, response.Message));
            return Ok(response.Model);
        }

        [HttpPost("add")]
        public async Task<ActionResult<GetFavoriteDto>> AddFavorite([FromBody] AddToFavoriteDto dto)
        {
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId))
            {
                return Unauthorized(new ApiResponse(401, "User ID not found in token."));
            }

            dto.UserId = userId;

            var response = await _favoriteService.AddFavoriteAsync(dto);
            if (!response.IsSucceeded)
            {
                return BadRequest(new ApiResponse(response.Status, response.Message));
            }


            var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new ApiResponse(404, "User not found."));
            }

            return Ok(response.Message);
        }



        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFavorite(int id)
        {
            var response = await _favoriteService.DeleteFavoriteAsync(User, id);
            if (!response.IsSucceeded)
                return BadRequest(new ApiResponse(response.Status, response.Message));
            return Ok(response.Message);
        }

        [HttpGet("user/favorites")]
        public async Task<ActionResult<PaginationResponse<GetFavoriteDto>>> GetUserFavorites([FromQuery] SpecParams specParams)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized("User ID not found in token.");
            }

            var spec = new EntitySpecification<Favorite>(specParams, e => e.UserId == userId);
            var entities = await _unitOfWork.Repository<Favorite>().GetAllPredicated(spec.Criteria, new[] { "User", "HandiCraft" });

            var response = _paginationService.Paginate(entities, specParams, e => new GetFavoriteDto
            {
                Id = e.Id,
                UserId = e.UserId,
                HandiCraftId = e.HandiCraftId,
                HandiCraftTitle = e.HandiCraft?.Title ?? "Unknown",
                CreatorProfilePicture = e.User?.ProfilePicture ?? "default.jpg",
                CreatorName = e.User != null ? $"{e.User.FirstName} {e.User.LastName}" : "Unknown User",
                DateCreated = e.DateCreated,
            });

            return Ok(response);
        }

    }
}
