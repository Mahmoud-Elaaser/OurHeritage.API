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

        // Get all handicrafts with pagination and filtering (Admin only)
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PaginationResponse<GetHandiCraftDto>>> GetAllHandiCrafts([FromQuery] SpecParams specParams)
        {
            var spec = new EntitySpecification<HandiCraft>(specParams, e =>
                (string.IsNullOrEmpty(specParams.Search) || e.Title.ToLower().Contains(specParams.Search.ToLower())));

            var entities = await _unitOfWork.Repository<HandiCraft>().ListAsync(spec);
            var response = _paginationService.Paginate<HandiCraft, GetHandiCraftDto>(entities, specParams, e => new GetHandiCraftDto
            {
                Id = e.Id,
                Title = e.Title,

                Description = e.Description
            });

            return Ok(response);
        }

        // Get a handicraft by ID
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

        // Create a new handicraft
        [HttpPost("create")]
        public async Task<IActionResult> CreateHandiCraft([FromForm] CreateOrUpdateHandiCraftDto createHandiCraftDto)
        {
            var response = await _handiCraftService.CreateHandiCraftAsync(createHandiCraftDto);
            if (!response.IsSucceeded)
                return BadRequest(new ApiResponse(response.Status, response.Message));

            return Ok(response.Message);
        }

        // Update an existing handicraft
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

        // Delete a handicraft (Admin only)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
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