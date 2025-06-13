using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OurHeritage.Core.Specifications;
using OurHeritage.Service.DTOs;
using OurHeritage.Service.Interfaces;

namespace OurHeritage.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StatisticsController : ControllerBase
    {
        private readonly IStatisticsService _statisticsService;

        public StatisticsController(IStatisticsService statisticsService)
        {
            _statisticsService = statisticsService;
        }


        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAdminStatistics()
        {
            var result = await _statisticsService.GetAdminStatisticsAsync();

            if (result.IsSucceeded)
                return Ok(result);

            return StatusCode(result.Status, result);
        }


        [HttpGet("top-users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetTopActiveUsers(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null)
        {
            var specParams = new SpecParams
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                Search = search
            };

            var result = await _statisticsService.GetTopActiveUsersPaginatedAsync(specParams);

            if (result.IsSucceeded)
                return Ok(result);

            return StatusCode(result.Status, result);
        }


        [HttpGet("popular-articles")]
        public async Task<IActionResult> GetPopularArticles(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null)
        {
            var specParams = new SpecParams
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                Search = search
            };

            var result = await _statisticsService.GetPopularArticlesPaginatedAsync(specParams);

            if (result.IsSucceeded)
                return Ok(result);

            return StatusCode(result.Status, result);
        }


        [HttpGet("popular-handicrafts")]
        public async Task<IActionResult> GetPopularHandiCrafts(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null)
        {
            var specParams = new SpecParams
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                Search = search
            };

            var result = await _statisticsService.GetPopularHandiCraftsPaginatedAsync(specParams);

            if (result.IsSucceeded)
                return Ok(result);

            return StatusCode(result.Status, result);
        }


        [HttpGet("categories")]
        public async Task<IActionResult> GetCategoryStatistics(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null)
        {
            var specParams = new SpecParams
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                Search = search
            };

            var result = await _statisticsService.GetCategoryStatisticsPaginatedAsync(specParams);

            if (result.IsSucceeded)
                return Ok(result);

            return StatusCode(result.Status, result);
        }


        [HttpGet("user-activity/{userId:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserActivityHistory(
            int userId,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null)
        {
            var specParams = new SpecParams
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                Search = search
            };

            var result = await _statisticsService.GetUserActivityHistoryAsync(userId, specParams);

            if (result.IsSucceeded)
                return Ok(result);

            return StatusCode(result.Status, result);
        }


        [HttpGet("users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null)
        {
            var specParams = new SpecParams
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                Search = search
            };

            var result = await _statisticsService.GetAllUsersPaginatedAsync(specParams);

            if (result.IsSucceeded)
                return Ok(result);

            return StatusCode(result.Status, result);
        }


        [HttpGet("content-engagement")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetContentEngagement(
            [FromQuery] string contentType = "all",
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null)
        {
            var specParams = new SpecParams
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                Search = search
            };

            var result = await _statisticsService.GetContentEngagementPaginatedAsync(specParams, contentType);

            if (result.IsSucceeded)
                return Ok(result);

            return StatusCode(result.Status, result);
        }


        [HttpGet("user/{userId:int}")]
        public async Task<IActionResult> GetUserStatistics(int userId)
        {
            var result = await _statisticsService.GetUserStatisticsAsync(userId);

            if (result.IsSucceeded)
                return Ok(result);

            return StatusCode(result.Status, result);
        }


        [HttpGet("monthly-report")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetMonthlyReport(
            [FromQuery] int year,
            [FromQuery] int month)
        {
            if (year < 2020 || year > DateTime.Now.Year + 1)
                return BadRequest(new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 400,
                    Message = "Invalid year parameter"
                });

            if (month < 1 || month > 12)
                return BadRequest(new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 400,
                    Message = "Invalid month parameter. Month should be between 1 and 12"
                });

            var result = await _statisticsService.GetMonthlyReportAsync(year, month);

            if (result.IsSucceeded)
                return Ok(result);

            return StatusCode(result.Status, result);
        }


        [HttpGet("categories/all")]
        public async Task<IActionResult> GetAllCategoryStatistics()
        {
            var result = await _statisticsService.GetCategoryStatisticsAsync();

            if (result.IsSucceeded)
                return Ok(result);

            return StatusCode(result.Status, result);
        }


        [HttpGet("engagement-summary")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetContentEngagementStats()
        {
            var result = await _statisticsService.GetContentEngagementStatsAsync();

            if (result.IsSucceeded)
                return Ok(result);

            return StatusCode(result.Status, result);
        }


        [HttpGet("current-month")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetCurrentMonthReport()
        {
            var now = DateTime.Now;
            var result = await _statisticsService.GetMonthlyReportAsync(now.Year, now.Month);

            if (result.IsSucceeded)
                return Ok(result);

            return StatusCode(result.Status, result);
        }


        [HttpGet("last-month")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetLastMonthReport()
        {
            var lastMonth = DateTime.Now.AddMonths(-1);
            var result = await _statisticsService.GetMonthlyReportAsync(lastMonth.Year, lastMonth.Month);

            if (result.IsSucceeded)
                return Ok(result);

            return StatusCode(result.Status, result);
        }
    }
}