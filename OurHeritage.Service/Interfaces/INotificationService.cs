using OurHeritage.Core.Entities;
using OurHeritage.Core.Specifications;
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
        Task<GenericResponseDto<NotificationDto>> CreateRepostNotificationAsync(int actorId, int recipientId, string message);

        Task<GenericResponseDto<PaginationResponse<NotificationDto>>> GetUnreadNotificationsAsync(int userId, int page = 1, int pageSize = 10);
        Task<ResponseDto> MarkNotificationAsReadAsync(int notificationId, int userId);
        Task<ResponseDto> MarkAllNotificationsAsReadAsync(int userId);
        Task<GenericResponseDto<PaginationResponse<NotificationDto>>> GetNotificationsByPredicateAsync(
            Expression<Func<Notification, bool>> predicate,
            string[] includes = null,
            int page = 1,
            int pageSize = 10);
        Task<ResponseDto> GetNotificationStatsAsync(int userId);
    }
}
