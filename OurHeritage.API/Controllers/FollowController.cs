using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OurHeritage.API.Response;
using OurHeritage.Service.DTOs;
using OurHeritage.Service.Interfaces;

namespace OurHeritage.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FollowController : ControllerBase
    {
        private readonly IFollowService _followService;

        public FollowController(IFollowService followService)
        {
            _followService = followService;
        }

        [HttpPost("follow")]
        public async Task<IActionResult> FollowUser(FollowDto createFollowDto)
        {
            var response = await _followService.FollowUserAsync(createFollowDto);
            if (!response.IsSucceeded)
                return BadRequest(new ApiResponse(response.Status, response.Message));

            return Ok(response.Message);
        }

        [HttpDelete("unfollow/{followerId}/{followingId}")]
        public async Task<IActionResult> UnfollowUser(FollowDto unFollowDto)
        {
            var response = await _followService.UnfollowUserAsync(unFollowDto);
            if (!response.IsSucceeded)
            {
                return BadRequest(new ApiResponse(response.Status, response.Message));
            }

            return Ok(response.Message);
        }



        [HttpGet("{userId}/followers")]
        public async Task<IActionResult> GetFollowers(int userId)
        {
            var response = await _followService.GetFollowersAsync(userId);
            if (!response.IsSucceeded)
                return BadRequest(new ApiResponse(response.Status, response.Message));

            return Ok(response.Models);
        }


        [HttpGet("{userId}/followings")]
        public async Task<IActionResult> GetFollowings(int userId)
        {
            var response = await _followService.GetFollowingAsync(userId);
            if (!response.IsSucceeded)
                return BadRequest(new ApiResponse(response.Status, response.Message));

            return Ok(response.Models);
        }
    }
}
