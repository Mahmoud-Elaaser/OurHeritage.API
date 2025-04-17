using Microsoft.AspNetCore.Mvc;
using OurHeritage.Service.DTOs.StoryDto;
using OurHeritage.Service.Interfaces;

namespace OurHeritage.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StoriesController : ControllerBase
    {
        private readonly IStoryService _storyService;

        public StoriesController(IStoryService storyService)
        {
            _storyService = storyService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateStory([FromBody] CreateOrUpdateStoryDto dto)
        {
            var result = await _storyService.CreateStoryAsync(dto);

            if (result.IsSucceeded)
                return StatusCode(result.Status, result);

            return BadRequest(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetStory(int id)
        {
            var result = await _storyService.GetStoryByIdAsync(id);

            if (result.IsSucceeded)
                return Ok(result);

            return NotFound(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllStories()
        {
            var result = await _storyService.GetAllStoriesAsync();
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStory(int id, [FromBody] CreateOrUpdateStoryDto dto)
        {
            var result = await _storyService.UpdateStoryAsync(id, dto);

            if (result.IsSucceeded)
                return Ok(result);

            if (result.Status == 404)
                return NotFound(result);

            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStory(int id)
        {
            var result = await _storyService.DeleteStoryAsync(User, id);

            if (result.IsSucceeded)
                return Ok(result);

            if (result.Status == 404)
                return NotFound(result);

            if (result.Status == 403)
                return Forbid();

            return BadRequest(result);
        }
    }
}