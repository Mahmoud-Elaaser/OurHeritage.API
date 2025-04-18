using Microsoft.AspNetCore.Mvc;
using OurHeritage.Service.Interfaces;

namespace OurHeritage.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HandicraftMatchingController : ControllerBase
    {
        private readonly IUserHandicraftMatchingService _matchingService;

        public HandicraftMatchingController(IUserHandicraftMatchingService matchingService)
        {
            _matchingService = matchingService;
        }

        //[HttpGet("find-crafters")]
        //public async Task<IActionResult> FindPerfectCrafters([FromQuery] string description, [FromQuery] int maxResults = 5)
        //{
        //    if (string.IsNullOrWhiteSpace(description))
        //    {
        //        return BadRequest("Description cannot be empty");
        //    }

        //    var results = await _matchingService.FindPerfectCraftersAsync(description, maxResults);

        //    var response = results.Select(r => new
        //    {
        //        UserId = r.User.Id,
        //        Name = $"{r.User.FirstName} {r.User.LastName}",
        //        ProfilePicture = r.User.ProfilePicture,
        //        Phone = r.User.Phone,
        //        Connections = r.User.Connections ?? new List<string>(),
        //        HandicraftCount = r.User.HandiCrafts?.Count ?? 0,
        //        //RelevantSkills = r.RelevantSkills,
        //        //MatchScore = r.MatchScore
        //    });

        //    return Ok(response);
        //}

        [HttpGet("recommend")]
        public async Task<IActionResult> RecommendUsers([FromQuery] string description)
        {
            var results = await _matchingService.FindTopUsersByHandiCraftCategoryAsync(description);
            return Ok(results);
        }


    }
}
