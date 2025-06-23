using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OurHeritage.Core.Specifications;
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

        public FavoritesController(IFavoriteService favoriteService)
        {
            _favoriteService = favoriteService;
        }

        [HttpGet("my-favorites")]
        public async Task<IActionResult> GetMyFavorites()
        {
            var response = await _favoriteService.GetUserFavoritesAsync(User);

            if (!response.IsSucceeded)
            {
                return StatusCode(response.Status, new { message = response.Message });
            }

            return Ok(response);
        }


        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserFavorites(
            int userId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null)
        {
            var specParams = new SpecParams
            {
                PageIndex = pageNumber,
                PageSize = pageSize,
                Search = search
            };

            var response = await _favoriteService.GetUserFavoritesAsync(userId, specParams);

            if (!response.IsSucceeded)
            {
                return BadRequest(new { message = response.Message });
            }

            return Ok(response);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetFavoriteById(int id)
        {
            var response = await _favoriteService.GetFavoriteByIdAsync(id);

            if (!response.IsSucceeded)
            {
                return NotFound(new { message = response.Message });
            }

            return Ok(response);
        }


        [HttpGet("handicraft/{handicraftId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetFavoritesForHandicraft(
            int handicraftId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null)
        {
            var specParams = new SpecParams
            {
                PageIndex = pageNumber,
                PageSize = pageSize,
                Search = search
            };

            var response = await _favoriteService.GetFavoritesForHandicraftAsync(handicraftId, specParams);

            if (!response.IsSucceeded)
            {
                return BadRequest(new { message = response.Message });
            }

            return Ok(response);
        }


        [HttpPost]
        public async Task<IActionResult> AddToFavorites([FromBody] AddToFavoriteDto addToFavoriteDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }


            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int loggedInUserId))
            {
                return Unauthorized(new { message = "User ID not found in token." });
            }
            addToFavoriteDto.UserId = loggedInUserId;


            var response = await _favoriteService.AddFavoriteAsync(addToFavoriteDto);

            if (!response.IsSucceeded)
            {
                return StatusCode(response.Status, new { message = response.Message });
            }

            return Ok(response.Model);
        }


        [HttpDelete("{favoriteId}")]
        public async Task<IActionResult> RemoveFavorite(int favoriteId)
        {
            var response = await _favoriteService.DeleteFavoriteAsync(User, favoriteId);

            if (!response.IsSucceeded)
            {
                return StatusCode(response.Status, new { message = response.Message });
            }

            return Ok(response.Message);
        }



        [HttpGet("check/{handicraftId}")]
        public async Task<IActionResult> CheckIfFavorited(int handicraftId)
        {
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId))
            {
                return Unauthorized(new { message = "User ID not found in token." });
            }

            var response = await _favoriteService.GetUserFavoritesAsync(userId, f => f.UserId == userId && f.HandiCraftId == handicraftId);

            if (!response.IsSucceeded)
            {
                return BadRequest(new { message = response.Message });
            }

            var favorites = response.Models as IEnumerable<object>;
            var isFavorited = favorites?.Any() == true;

            return Ok(new { isFavorited });
        }
    }
}