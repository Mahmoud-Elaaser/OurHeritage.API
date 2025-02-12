using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OurHeritage.API.Response;
using OurHeritage.Service.DTOs.HandiCraftDto;
using OurHeritage.Service.Interfaces;

namespace OurHeritage.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class HandiCraftsController : ControllerBase
    {
        private readonly IHandiCraftService _handiCraftService;

        public HandiCraftsController(IHandiCraftService handiCraftService)
        {
            _handiCraftService = handiCraftService;
        }

        [HttpGet]
        //[Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<GetHandiCraftDto>>> GetAllHandiCrafts()
        {
            var handiCrafts = await _handiCraftService.GetAllHandiCraftsAsync();
            if (!handiCrafts.IsSucceeded)
            {
                return BadRequest(new ApiResponse(handiCrafts.Status, handiCrafts.Message));
            }
            return Ok(handiCrafts.Models);
        }


        [HttpGet("{id}")]

        public async Task<IActionResult> GetHandiCraftById(int id)
        {
            var handiCraft = await _handiCraftService.GetHandiCraftByIdAsync(id);
            if (!handiCraft.IsSucceeded)
            {
                return BadRequest(new ApiResponse(handiCraft.Status, handiCraft.Message));
            }
            return Ok(handiCraft.Model);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateHandiCraft(CreateOrUpdateHandiCraftDto createHandiCraftDto)
        {
            var response = await _handiCraftService.CreateHandiCraftAsync(createHandiCraftDto);
            if (!response.IsSucceeded)
                return BadRequest(new ApiResponse(response.Status, response.Message));

            return Ok(response.Message);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateHandiCraft(int id, [FromBody] CreateOrUpdateHandiCraftDto updateHandiCraftDto)
        {
            var handiCraft = await _handiCraftService.UpdateHandiCraftAsync(id, updateHandiCraftDto);
            if (!handiCraft.IsSucceeded)
            {
                return BadRequest(new ApiResponse(handiCraft.Status, handiCraft.Message));
            }
            return Ok(handiCraft.Message);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
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
