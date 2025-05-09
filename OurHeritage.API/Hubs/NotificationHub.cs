using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using OurHeritage.Service.DTOs;
using OurHeritage.Service.DTOs.NotificationsDto;
using OurHeritage.Service.Interfaces;
using System.Security.Claims;

namespace OurHeritage.API.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly INotificationService _notificationService;
        private static readonly Dictionary<int, string> _userConnections = new Dictionary<int, string>();

        public NotificationHub(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public override async Task OnConnectedAsync()
        {
            int userId = int.Parse(Context.User.FindFirstValue(ClaimTypes.NameIdentifier));
            string connectionId = Context.ConnectionId;

            // Store the connection
            lock (_userConnections)
            {
                if (_userConnections.ContainsKey(userId))
                {
                    _userConnections[userId] = connectionId;
                }
                else
                {
                    _userConnections.Add(userId, connectionId);
                }
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            int userId = int.Parse(Context.User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Remove the connection
            lock (_userConnections)
            {
                if (_userConnections.ContainsKey(userId))
                {
                    _userConnections.Remove(userId);
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        // Send a notification to a specific user
        public async Task SendNotificationToUser(int userId, NotificationDto notification)
        {
            lock (_userConnections)
            {
                if (_userConnections.TryGetValue(userId, out string connectionId))
                {
                    Clients.Client(connectionId).SendAsync("ReceiveNotification", notification);
                }
            }
        }

        public async Task NotifyUserFollowed(int followedUserId)
        {
            int followerId = int.Parse(Context.User.FindFirstValue(ClaimTypes.NameIdentifier));
            string followerFirstName = Context.User.FindFirstValue(ClaimTypes.GivenName);
            string followerLastName = Context.User.FindFirstValue(ClaimTypes.Surname);

            // Create and save notification to database
            var result = await _notificationService.CreateFollowNotificationAsync(
                followerId,
                followedUserId,
                $"{followerFirstName} {followerLastName} started following you");

            if (result.Success && result.Data != null)
            {
                // Send real-time notification if user is online
                await SendNotificationToUser(followedUserId, result.Data);
            }
        }

        public async Task NotifyArticleLiked(int articleId)
        {
            int likerId = int.Parse(Context.User.FindFirstValue(ClaimTypes.NameIdentifier));
            string likerFirstName = Context.User.FindFirstValue(ClaimTypes.GivenName);
            string likerLastName = Context.User.FindFirstValue(ClaimTypes.Surname);

            var result = await _notificationService.CreateArticleLikeNotificationAsync(
                likerId,
                articleId,
                $"{likerFirstName} {likerLastName} liked your article");

            if (result.Success && result.Data != null && result.Data.RecipientId.HasValue)
            {
                await SendNotificationToUser(result.Data.RecipientId.Value, result.Data);
            }
        }

        public async Task NotifyArticleCommented(int articleId, string commentContent)
        {
            int commenterId = int.Parse(Context.User.FindFirstValue(ClaimTypes.NameIdentifier));
            string commenterFirstName = Context.User.FindFirstValue(ClaimTypes.GivenName);
            string commenterLastName = Context.User.FindFirstValue(ClaimTypes.Surname);

            // Truncate comment content for notification preview
            string commentPreview = TruncateContent(commentContent, 50);

            var result = await _notificationService.CreateArticleCommentNotificationAsync(
                commenterId,
                articleId,
                $"{commenterFirstName} {commenterLastName} commented on your article");

            if (result.Success && result.Data != null && result.Data.RecipientId.HasValue)
            {
                await SendNotificationToUser(result.Data.RecipientId.Value, result.Data);
            }
        }

        public async Task NotifyArticleReposted(int articleId)
        {
            int reposterId = int.Parse(Context.User.FindFirstValue(ClaimTypes.NameIdentifier));
            string reposterFirstName = Context.User.FindFirstValue(ClaimTypes.GivenName);
            string reposterLastName = Context.User.FindFirstValue(ClaimTypes.Surname);


            string message = $"{reposterFirstName} {reposterLastName} reposted your article";

            var result = await _notificationService.CreateRepostNotificationAsync(
                reposterId,
                articleId,
                message);

            if (result.Success && result.Data != null && result.Data.RecipientId.HasValue)
            {
                await SendNotificationToUser(result.Data.RecipientId.Value, result.Data);
            }
        }


        public async Task<List<NotificationDto>> GetUnreadNotifications()
        {
            int userId = int.Parse(Context.User.FindFirstValue(ClaimTypes.NameIdentifier));
            var response = await _notificationService.GetUnreadNotificationsAsync(userId);

            if (response.Success && response.Data?.Items != null)
            {
                return response.Data.Items.ToList();
            }

            return new List<NotificationDto>();
        }


        public async Task<NotificationStatsDto> GetNotificationStats()
        {
            int userId = int.Parse(Context.User.FindFirstValue(ClaimTypes.NameIdentifier));
            var response = await _notificationService.GetNotificationStatsAsync(userId);

            if (response.IsSucceeded && response.Model != null)
            {
                return response.Model as NotificationStatsDto;
            }

            return new NotificationStatsDto { UserId = userId, UnreadCount = 0, TotalCount = 0 };
        }

        public async Task<bool> MarkNotificationAsRead(int notificationId)
        {
            int userId = int.Parse(Context.User.FindFirstValue(ClaimTypes.NameIdentifier));
            var response = await _notificationService.MarkNotificationAsReadAsync(notificationId, userId);
            return response.IsSucceeded;
        }

        public async Task<bool> MarkAllNotificationsAsRead()
        {
            int userId = int.Parse(Context.User.FindFirstValue(ClaimTypes.NameIdentifier));
            var response = await _notificationService.MarkAllNotificationsAsReadAsync(userId);
            return response.IsSucceeded;
        }

        private string TruncateContent(string content, int maxLength)
        {
            if (string.IsNullOrEmpty(content) || content.Length <= maxLength)
                return content;

            return content.Substring(0, maxLength) + "...";
        }
    }
}