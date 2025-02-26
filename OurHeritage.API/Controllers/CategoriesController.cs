using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OurHeritage.API.Response;
using OurHeritage.Core.Entities;
using OurHeritage.Core.Specifications;
using OurHeritage.Repo.Repositories.Interfaces;
using OurHeritage.Service.DTOs.CategoryDto;
using OurHeritage.Service.Interfaces;

namespace OurHeritage.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPaginationService _paginationService;

        public CategoriesController(ICategoryService categoryService, IUnitOfWork unitOfWork, IPaginationService paginationService)
        {
            _categoryService = categoryService;
            _unitOfWork = unitOfWork;
            _paginationService = paginationService;
        }

        // Get all categories with pagination and filtering (Admin only)
        [HttpGet]
     //   [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PaginationResponse<GetCategoryDto>>> GetAllCategories([FromQuery] SpecParams specParams)
        {
            var spec = new EntitySpecification<Category>(specParams, e =>
                (string.IsNullOrEmpty(specParams.Search) || e.Name.ToLower().Contains(specParams.Search.ToLower())));

            var entities = await _unitOfWork.Repository<Category>().ListAsync(spec);
            var totalEntities = await _unitOfWork.Repository<Category>().CountAsync(spec);

            var response = _paginationService.Paginate<Category, GetCategoryDto>(entities, specParams, e => new GetCategoryDto
            {
                Id = e.Id,
                Name = e.Name,
            });

            return Ok(response);
        }

        // Get a category by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (!category.IsSucceeded)
            {
                return NotFound(new ApiResponse(category.Status, category.Message));
            }
            return Ok(category.Model);
        }

        // Create a new category
        [HttpPost("create")]
        public async Task<IActionResult> CreateCategory([FromForm] CreateOrUpdateCategoryDto createCategoryDto)
        {
            var response = await _categoryService.CreateCategoryAsync(createCategoryDto);
            if (!response.IsSucceeded)
                return BadRequest(new ApiResponse(response.Status, response.Message));

            return Ok(response.Message);
        }

        // Update an existing category
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromForm] CreateOrUpdateCategoryDto updateCategoryDto)
        {
            var category = await _categoryService.UpdateCategoryAsync(id, updateCategoryDto);
            if (!category.IsSucceeded)
            {
                return BadRequest(new ApiResponse(category.Status, category.Message));
            }
            return Ok(category.Message);
        }

        // Delete a category (Admin only)
        [HttpDelete("{id}")]
   //     [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _categoryService.DeleteCategoryAsync(id);
            if (!category.IsSucceeded)
            {
                return BadRequest(new ApiResponse(category.Status, category.Message));
            }

            return Ok(category.Message);
        }
    }
}
