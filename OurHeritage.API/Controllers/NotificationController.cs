using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using OurHeritage.API.Hubs;
using OurHeritage.Service.DTOs;
using OurHeritage.Service.Interfaces;
using System.Security.Claims;

namespace OurHeritage.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly IHubContext<NotificationHub> _notificationHubContext;

        public NotificationController(
            INotificationService notificationService,
            IHubContext<NotificationHub> notificationHubContext)
        {
            _notificationService = notificationService;
            _notificationHubContext = notificationHubContext;
        }

        [HttpGet]
        public async Task<ActionResult<ResponseDto>> GetUnreadNotifications()
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var response = await _notificationService.GetUnreadNotificationsAsync(userId);

            if (!response.Success)
            {
                return StatusCode(400, response);
            }

            return Ok(response);
        }

        [HttpGet("stats")]
        public async Task<ActionResult<ResponseDto>> GetNotificationStats()
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var response = await _notificationService.GetNotificationStatsAsync(userId);

            if (!response.IsSucceeded)
            {
                return StatusCode(response.Status, response);
            }

            return Ok(response);
        }

        [HttpPut("{id}/read")]
        public async Task<ActionResult<ResponseDto>> MarkAsRead(int id)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var response = await _notificationService.MarkNotificationAsReadAsync(id, userId);

            if (!response.IsSucceeded)
            {
                return StatusCode(response.Status, response);
            }

            return Ok(response);
        }

        [HttpPut("read-all")]
        public async Task<ActionResult<ResponseDto>> MarkAllAsRead()
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var response = await _notificationService.MarkAllNotificationsAsReadAsync(userId);

            if (!response.IsSucceeded)
            {
                return StatusCode(response.Status, response);
            }

            return Ok(response);
        }
    }
}