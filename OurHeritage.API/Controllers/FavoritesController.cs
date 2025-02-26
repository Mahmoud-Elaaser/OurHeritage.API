using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OurHeritage.Core.Entities;
using OurHeritage.Service.DTOs.FavoriteDto;
using OurHeritage.Service.Interfaces;
using System.Security.Claims;

namespace OurHeritage.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FavoritesController : ControllerBase
    {
        private readonly IFavoriteService _favoriteService;

        public FavoritesController(IFavoriteService favoriteService)
        {
            _favoriteService = favoriteService;
        }


        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<GetFavoriteDto>>> GetAllFavorites()
        {
            var favorites = await _favoriteService.GetAllFavoritesAsync();
            var viewModels = favorites.Select(f => new GetFavoriteDto
            {
                Id = f.Id,
                UserId = f.UserId,
                HandiCraftId = f.HandiCraftId
            });

            return Ok(viewModels);
        }

        [HttpGet("user")]
        public async Task<ActionResult<IEnumerable<GetFavoriteDto>>> GetUserFavorites()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var favorites = await _favoriteService.GetFavoritesByUserIdAsync(userId);
            var viewModels = favorites.Select(f => new GetFavoriteDto
            {
                Id = f.Id,
                UserId = f.UserId,
                HandiCraftId = f.HandiCraftId
            });

            return Ok(viewModels);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GetFavoriteDto>> GetFavorite(int id)
        {
            var favorite = await _favoriteService.GetFavoriteByIdAsync(id);

            if (favorite == null)
            {
                return NotFound();
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            // Check if the user is the owner of the favorite or an admin
            if (favorite.UserId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            var viewModel = new GetFavoriteDto
            {
                Id = favorite.Id,
                UserId = favorite.UserId,
                HandiCraftId = favorite.HandiCraftId
            };

            return Ok(viewModel);
        }

        [HttpPost]
        public async Task<ActionResult<GetFavoriteDto>> AddFavorite(AddToFavoriteDto model)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var favorite = new Favorite
            {
                UserId = userId,
                HandiCraftId = model.HandiCraftId
            };

            var createdFavorite = await _favoriteService.AddFavoriteAsync(favorite);

            var viewModel = new GetFavoriteDto
            {
                Id = createdFavorite.Id,
                UserId = createdFavorite.UserId,
                HandiCraftId = createdFavorite.HandiCraftId
            };

            return CreatedAtAction(nameof(GetFavorite), new { id = createdFavorite.Id }, viewModel);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveFavorite(int id)
        {
            var favorite = await _favoriteService.GetFavoriteByIdAsync(id);

            if (favorite == null)
            {
                return NotFound();
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            // Check if the user is the owner of the favorite or an admin
            if (favorite.UserId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            var result = await _favoriteService.RemoveFavoriteAsync(id);

            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpGet("check/{handiCraftId}")]
        public async Task<ActionResult<bool>> CheckFavorite(int handiCraftId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var isFavorite = await _favoriteService.IsFavoriteAsync(userId, handiCraftId);

            return Ok(isFavorite);
        }
    }
}