using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OurHeritage.API.Response;
using OurHeritage.Service.DTOs.UserDto;
using OurHeritage.Service.Interfaces;

namespace OurHeritage.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }


        [HttpGet]
        //[Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<GetUserDto>>> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            if (!users.IsSucceeded)
            {
                return BadRequest(new ApiResponse(users.Status, users.Message));
            }
            return Ok(users.Models);
        }



        [HttpGet("{id}")]
        [ProducesResponseType(typeof(GetUserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (!user.IsSucceeded)
            {
                return BadRequest(new ApiResponse(user.Status, user.Message));
            }
            return Ok(user.Model);
        }


        [HttpPut("{id}")]
        [ProducesResponseType(typeof(CreateOrUpdateUserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] CreateOrUpdateUserDto dto)
        {
            var user = await _userService.UpdateUserAsync(id, dto);
            if (!user.IsSucceeded)
            {
                return BadRequest(new ApiResponse(user.Status, user.Message));
            }
            return Ok(user.Message);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _userService.DeleteUserAsync(id);
            if (!user.IsSucceeded)
            {
                return BadRequest(new ApiResponse(user.Status, user.Message));
            }

            return Ok(user.Message);
        }
    }
}
