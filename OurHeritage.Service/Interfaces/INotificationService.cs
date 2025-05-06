using OurHeritage.Core.Entities;
using OurHeritage.Service.DTOs;
using OurHeritage.Service.DTOs.CulturalArticleDto;
using OurHeritage.Service.DTOs.NotificationsDto;
using System.Linq.Expressions;

namespace OurHeritage.Service.Interfaces
{
    public interface INotificationService
    {
        Task<GenericResponseDto<NotificationDto>> CreateFollowNotificationAsync(int actorId, int recipientId, string message);
        Task<GenericResponseDto<NotificationDto>> CreateArticleLikeNotificationAsync(int actorId, int articleId, string message);
        Task<GenericResponseDto<NotificationDto>> CreateArticleCommentNotificationAsync(int actorId, int articleId, string message);
        Task<GenericResponseDto<List<NotificationDto>>> GetUnreadNotificationsAsync(int userId);
        Task<ResponseDto> MarkNotificationAsReadAsync(int notificationId, int userId);
        Task<ResponseDto> MarkAllNotificationsAsReadAsync(int userId);
        Task<ResponseDto> GetNotificationsByPredicateAsync(Expression<Func<Notification, bool>> predicate, string[] includes = null);
        Task<ResponseDto> GetNotificationStatsAsync(int userId);
    }
}
