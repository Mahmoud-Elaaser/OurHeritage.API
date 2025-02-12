using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OurHeritage.API.Response;
using OurHeritage.Service.DTOs.CategoryDto;
using OurHeritage.Service.Interfaces;

namespace OurHeritage.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        //[Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<GetCategoryDto>>> GetAllCategories()
        {
            var Categorys = await _categoryService.GetAllCategoriesAsync();
            if (!Categorys.IsSucceeded)
            {
                return BadRequest(new ApiResponse(Categorys.Status, Categorys.Message));
            }
            return Ok(Categorys.Models);
        }


        [HttpGet("{id}")]
        [ProducesResponseType(typeof(GetCategoryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            var Category = await _categoryService.GetCategoryByIdAsync(id);
            if (!Category.IsSucceeded)
            {
                return BadRequest(new ApiResponse(Category.Status, Category.Message));
            }
            return Ok(Category.Model);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateCategory([FromBody] CreateOrUpdateCategoryDto createCategoryDto)
        {
            var response = await _categoryService.CreateCategoryAsync(createCategoryDto);
            if (!response.IsSucceeded)
                return BadRequest(new ApiResponse(response.Status, response.Message));

            return Ok(response.Message);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(CreateOrUpdateCategoryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CreateOrUpdateCategoryDto updateCategoryDto)
        {
            var category = await _categoryService.UpdateCategoryAsync(id, updateCategoryDto);
            if (!category.IsSucceeded)
            {
                return BadRequest(new ApiResponse(category.Status, category.Message));
            }
            return Ok(category.Message);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [Authorize(Roles = "Admin")]
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
