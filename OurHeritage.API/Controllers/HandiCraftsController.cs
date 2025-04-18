using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OurHeritage.API.Response;
using OurHeritage.Core.Entities;
using OurHeritage.Core.Specifications;
using OurHeritage.Repo.Repositories.Interfaces;
using OurHeritage.Service.DTOs.FavoriteDto;
using OurHeritage.Service.DTOs.HandiCraftDto;
using OurHeritage.Service.Helper;
using OurHeritage.Service.Interfaces;
using System.Security.Claims;

namespace OurHeritage.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class HandiCraftsController : ControllerBase
    {
        private readonly IHandiCraftService _handiCraftService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPaginationService _paginationService;

        public HandiCraftsController(IHandiCraftService handiCraftService, IUnitOfWork unitOfWork, IPaginationService paginationService)
        {
            _handiCraftService = handiCraftService;
            _unitOfWork = unitOfWork;
            _paginationService = paginationService;
        }

        [HttpGet]
        [HttpGet]
        public async Task<ActionResult<PaginationResponse<GetHandiCraftDto>>> GetAllHandiCrafts([FromQuery] SpecParams specParams)
        {
            var spec = new EntitySpecification<HandiCraft>(specParams, e =>
                string.IsNullOrEmpty(specParams.Search) || e.Title.ToLower().Contains(specParams.Search.ToLower()));


            var entities = await _unitOfWork.Repository<HandiCraft>()
                .GetAllPredicated(spec.Criteria, new[] { "User", "Category", "Favorite.User" });

            var response = _paginationService.Paginate(entities, specParams, e => new GetHandiCraftDto
            {
                Id = e.Id,
                Title = e.Title,
                CategoryId = e.CategoryId,
                ImageOrVideo = e.ImageOrVideo,
                Price = e.Price,
                UserId = e.UserId,
                Description = e.Description,
                NameOfUser = e.User != null ? $"{e.User.FirstName} {e.User.LastName}" : "Unknown User",
                UserProfilePicture = e.User?.ProfilePicture ?? "default.jpg",
                CategoryName = e.Category.Name,
                DateAdded = e.DateAdded.ToString("yyyy-MM-dd"),
                TimeAgo = TimeAgoHelper.GetTimeAgo(e.DateAdded),
                FavoriteCount = e.Favorite?.Count ?? 0,
                FavoritedBy = e.Favorite?.Select(fav => new GetFavoriteDto
                {
                    Id = fav.Id,
                    HandiCraftId = fav.HandiCraftId,
                    UserId = fav.UserId,
                    CreatorName = fav.User != null ? $"{fav.User.FirstName} {fav.User.LastName}" : "Unknown User",
                    HandiCraftTitle = e.Title,
                    CreatorProfilePicture = fav.User?.ProfilePicture ?? "default.jpg",
                    DateCreated = fav.DateCreated.ToString("yyyy-MM-dd")
                }).ToList() ?? new List<GetFavoriteDto>()
            });

            return Ok(response);
        }




        [HttpGet("{id}")]
        public async Task<IActionResult> GetHandiCraftById(int id)
        {
            var handiCraft = await _handiCraftService.GetHandiCraftByIdAsync(id);
            if (!handiCraft.IsSucceeded)
            {
                return NotFound(new ApiResponse(handiCraft.Status, handiCraft.Message));
            }
            return Ok(handiCraft.Model);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateHandiCraft([FromForm] CreateOrUpdateHandiCraftDto createHandiCraftDto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                createHandiCraftDto.UserId = userId;
            }
            else
            {
                return Unauthorized("Valid User ID is required");
            }
            var response = await _handiCraftService.CreateHandiCraftAsync(createHandiCraftDto);
            if (!response.IsSucceeded)
                return BadRequest(new ApiResponse(response.Status, response.Message));

            return Ok(response.Message);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateHandiCraft(int id, [FromForm] CreateOrUpdateHandiCraftDto updateHandiCraftDto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                updateHandiCraftDto.UserId = userId;
            }
            else
            {
                return Unauthorized("Valid User ID is required");
            }
            var handiCraft = await _handiCraftService.UpdateHandiCraftAsync(id, updateHandiCraftDto);
            if (!handiCraft.IsSucceeded)
            {
                return BadRequest(new ApiResponse(handiCraft.Status, handiCraft.Message));
            }
            return Ok(handiCraft.Message);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHandiCraft(int id)
        {
            var handiCraft = await _handiCraftService.DeleteHandiCraftAsync(User, id);
            if (!handiCraft.IsSucceeded)
            {
                return BadRequest(new ApiResponse(handiCraft.Status, handiCraft.Message));
            }

            return Ok(handiCraft.Message);
        }
    }
}