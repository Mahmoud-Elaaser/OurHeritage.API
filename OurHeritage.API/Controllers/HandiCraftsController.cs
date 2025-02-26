using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OurHeritage.API.Response;
using OurHeritage.Core.Entities;
using OurHeritage.Core.Specifications;
using OurHeritage.Repo.Repositories.Interfaces;
using OurHeritage.Service.DTOs.HandiCraftDto;
using OurHeritage.Service.Interfaces;

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
        public async Task<ActionResult<PaginationResponse<GetHandiCraftDto>>> GetAllHandiCrafts([FromQuery] SpecParams specParams)
        {
            var spec = new EntitySpecification<HandiCraft>(specParams, e =>
                string.IsNullOrEmpty(specParams.Search) || e.Title.ToLower().Contains(specParams.Search.ToLower()));

            // Include User entity to fetch creator details
            var entities = await _unitOfWork.Repository<HandiCraft>()
                .GetAllPredicated(spec.Criteria, new[] { "User" });


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
                UserProfilePicture = e.User?.ProfilePicture ?? "default.jpg"
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
            var response = await _handiCraftService.CreateHandiCraftAsync(createHandiCraftDto);
            if (!response.IsSucceeded)
                return BadRequest(new ApiResponse(response.Status, response.Message));

            return Ok(response.Message);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateHandiCraft(int id, [FromForm] CreateOrUpdateHandiCraftDto updateHandiCraftDto)
        {
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
            var handiCraft = await _handiCraftService.DeleteHandiCraftAsync(id);
            if (!handiCraft.IsSucceeded)
            {
                return BadRequest(new ApiResponse(handiCraft.Status, handiCraft.Message));
            }

            return Ok(handiCraft.Message);
        }
    }
}