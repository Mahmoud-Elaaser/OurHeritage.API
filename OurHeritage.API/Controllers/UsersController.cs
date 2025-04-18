﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OurHeritage.API.Response;
using OurHeritage.Core.Entities;
using OurHeritage.Core.Specifications;
using OurHeritage.Repo.Repositories.Interfaces;
using OurHeritage.Service.DTOs.UserDto;
using OurHeritage.Service.Interfaces;
using System.Security.Claims;

namespace OurHeritage.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPaginationService _paginationService;

        public UsersController(IUserService userService, IPaginationService paginationService, IUnitOfWork unitOfWork)
        {
            _userService = userService;
            _paginationService = paginationService;
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        // [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PaginationResponse<GetUserDto>>> GetAllUsers([FromQuery] SpecParams specParams)
        {
            var spec = new EntitySpecification<User>(specParams, e =>
                (string.IsNullOrEmpty(specParams.Search) || e.UserName.ToLower().Contains(specParams.Search.ToLower())));

            var entities = await _unitOfWork.Repository<User>().ListAsync(spec);
            var response = _paginationService.Paginate<User, GetUserDto>(entities, specParams, e => new GetUserDto
            {
                Id = e.Id,
                FirstName = e.FirstName,
                LastName = e.LastName,
                FullName = e.FirstName + e.LastName,
                ProfilePicture = e.ProfilePicture,
                CoverProfilePicture = e.CoverProfilePicture,
                Phone = e.Phone,
                Skills = e.Skills,
                Connections = e.Connections,
                DateJoined = e.DateJoined,
                Gender = (Core.Enums.Gender)Enum.Parse(typeof(OurHeritage.Core.Enums.Gender), e.Gender, true)

            });

            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var response = await _userService.GetUserByIdAsync(id);
            if (!response.IsSucceeded)
            {
                return NotFound(new ApiResponse(response.Status, response.Message));
            }
            return Ok(response.Model);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromForm] CreateOrUpdateUserDto dto)
        {
            var response = await _userService.UpdateUserAsync(id, dto);
            if (!response.IsSucceeded)
            {
                return BadRequest(new ApiResponse(response.Status, response.Message));
            }
            return Ok(response.Message);
        }

        [HttpPost("profile-picture")]
        public async Task<IActionResult> UpdateProfilePicture([FromForm] UpdateProfilePicDto dto)
        {
            var tokenId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userId = int.Parse(tokenId);
            var result = await _userService.UpdateProfilePictureAsync(userId, dto);
            return Ok(result.Message);
        }


        [HttpPost("cover-photo")]
        public async Task<IActionResult> UpdateCoverPhoto([FromForm] UpdateCoverPhotoDto dto)
        {
            var tokenId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userId = int.Parse(tokenId);
            var result = await _userService.UpdateCoverPhotoAsync(userId, dto);
            return Ok(result.Message);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var response = await _userService.DeleteUserAsync(User, id);
            if (!response.IsSucceeded)
            {
                return BadRequest(new ApiResponse(response.Status, response.Message));
            }

            return Ok(response.Message);
        }


        [HttpGet("suggested-friends")]
        public async Task<IActionResult> GetSuggestedFriends()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized("User ID not found in token.");
            }
            var response = await _userService.GetSuggestedFriendsAsync(userId);
            return Ok(response.Models);
        }


    }
}