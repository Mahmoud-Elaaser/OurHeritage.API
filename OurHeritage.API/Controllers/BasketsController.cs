using Microsoft.AspNetCore.Mvc;
using OurHeritage.Service.DTOs;
using OurHeritage.Service.Interfaces;
using System.Security.Claims;

namespace OurHeritage.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BasketsController : ControllerBase
    {
        private readonly IBasketService _basketService;
        private readonly ILogger<BasketsController> _logger;

        public BasketsController(IBasketService basketService, ILogger<BasketsController> logger)
        {
            _basketService = basketService;
            _logger = logger;
        }


        [HttpPost("add")]
        public async Task<IActionResult> AddToBasketAsync(int handiCraftId, int quantity)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new ResponseDto
                    {
                        IsSucceeded = false,
                        Message = "User is not authorized or user ID is missing."
                    });
                }


                var response = await _basketService.AddToBasketAsync(userId, handiCraftId, quantity);

                if (response.IsSucceeded)
                {
                    return Ok(response.Message);
                }

                return BadRequest(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding item to basket for user {UserId}", User.Identity.Name);
                return StatusCode(500, new ResponseDto
                {
                    IsSucceeded = false,
                    Message = "An error occurred while adding the item to the basket."
                });
            }
        }



        [HttpGet("{id}")]
        public async Task<IActionResult> GetItemByIdAsync(int id)
        {
            try
            {
                var response = await _basketService.GetItemByIdAsync(id);

                if (response.IsSucceeded)
                {
                    return Ok(response.Model);
                }

                return NotFound(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching basket item with ID {Id}", id);
                return StatusCode(500, new ResponseDto
                {
                    IsSucceeded = false,
                    Message = "An error occurred while fetching the basket item."
                });
            }
        }


        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetItemsForUserAsync(int userId)
        {
            try
            {
                var response = await _basketService.GetItemsForUserAsync(userId);

                if (response.IsSucceeded)
                {
                    return Ok(response.Models);
                }

                return NotFound(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching basket items for user {UserId}", userId);
                return StatusCode(500, new ResponseDto
                {
                    IsSucceeded = false,
                    Message = "An error occurred while fetching the basket items."
                });
            }
        }

        [HttpGet("Items-for-logged-in-user")]
        public async Task<IActionResult> GetItemsForLoggedUserAsync()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new ResponseDto
                    {
                        IsSucceeded = false,
                        Message = "User is not authorized or user ID is missing."
                    });
                }
                var response = await _basketService.GetItemsForUserAsync(userId);

                if (response.IsSucceeded)
                {
                    return Ok(response.Models);
                }

                return NotFound(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching basket items for user UserId");
                return StatusCode(500, new ResponseDto
                {
                    IsSucceeded = false,
                    Message = "An error occurred while fetching the basket items."
                });
            }
        }


        [HttpPut("{basketItemId}")]
        public async Task<IActionResult> UpdateBasketItemAsync(int basketItemId, int quantity)
        {
            try
            {
                var response = await _basketService.UpdateBasketItemAsync(basketItemId, quantity);

                if (response.IsSucceeded)
                {
                    return Ok(response.Message);
                }

                return NotFound(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating basket item with ID {BasketItemId}", basketItemId);
                return StatusCode(500, new ResponseDto
                {
                    IsSucceeded = false,
                    Message = "An error occurred while updating the basket item."
                });
            }
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItemAsync(int id)
        {
            try
            {
                var response = await _basketService.DeleteItemAsync(id);

                if (response.IsSucceeded)
                {
                    return Ok(response.Message);
                }

                return NotFound(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting basket item with ID {Id}", id);
                return StatusCode(500, new ResponseDto
                {
                    IsSucceeded = false,
                    Message = "An error occurred while deleting the basket item."
                });
            }
        }
    }
}
