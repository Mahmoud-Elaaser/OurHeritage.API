using AutoMapper;
using OurHeritage.Core.Entities;
using OurHeritage.Core.Enums;
using OurHeritage.Core.Specifications;
using OurHeritage.Repo.Repositories.Interfaces;
using OurHeritage.Service.DTOs;
using OurHeritage.Service.DTOs.ChatDto;
using OurHeritage.Service.DTOs.CulturalArticleDto;
using OurHeritage.Service.DTOs.NotificationsDto;
using OurHeritage.Service.Interfaces;
using System.Linq.Expressions;

namespace OurHeritage.Service.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IPaginationService _paginationService;
        private readonly IGenericRepository<Notification> _notificationRepository;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<CulturalArticle> _articleRepository;

        public NotificationService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IGenericRepository<Notification> notificationRepository,
            IGenericRepository<User> userRepository,
            IGenericRepository<CulturalArticle> articleRepository,
            IPaginationService paginationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _notificationRepository = notificationRepository;
            _userRepository = userRepository;
            _articleRepository = articleRepository;
            _paginationService = paginationService;
        }

        public async Task<GenericResponseDto<NotificationDto>> CreateFollowNotificationAsync(int actorId, int recipientId, string message)
        {
            // Validate users exist
            var actor = await _userRepository.GetByIdAsync(actorId);
            var recipient = await _userRepository.GetByIdAsync(recipientId);

            if (actor == null || recipient == null)
            {
                return new GenericResponseDto<NotificationDto>
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            // Don't notify if user follows themselves
            if (actorId == recipientId)
            {
                return new GenericResponseDto<NotificationDto>
                {
                    Success = false,
                    Message = "Cannot create notification for self-follow"
                };
            }

            var notification = new Notification
            {
                ActorId = actorId,
                RecipientId = recipientId,
                Type = NotificationType.Follow,
                Message = message,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            await _notificationRepository.AddAsync(notification);
            await _unitOfWork.CompleteAsync();

            var notificationDto = new NotificationDto
            {
                Id = notification.Id,
                RecipientId = notification.RecipientId,
                ActorId = notification.ActorId,
                Actor = new UserPreviewDto
                {
                    Id = actor.Id,
                    FirstName = actor.FirstName,
                    LastName = actor.LastName,
                    ProfilePicture = actor.ProfilePicture
                },
                Type = notification.Type,
                Message = notification.Message,
                CreatedAt = notification.CreatedAt,
                IsRead = notification.IsRead
            };

            return new GenericResponseDto<NotificationDto>
            {
                Success = true,
                Message = "Follow notification created successfully",
                Data = notificationDto
            };
        }

        public async Task<GenericResponseDto<NotificationDto>> CreateArticleLikeNotificationAsync(int actorId, int articleId, string message)
        {
            var actor = await _userRepository.GetByIdAsync(actorId);
            if (actor == null)
            {
                return new GenericResponseDto<NotificationDto>
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            var article = await _articleRepository.GetByIdAsync(articleId);
            if (article == null)
            {
                return new GenericResponseDto<NotificationDto>
                {
                    Success = false,
                    Message = "Article not found"
                };
            }

            if (article.UserId == actorId)
            {
                return new GenericResponseDto<NotificationDto>
                {
                    Success = false,
                    Message = "Cannot create notification for self-like"
                };
            }

            var notification = new Notification
            {
                ActorId = actorId,
                RecipientId = article.UserId,
                Type = NotificationType.ArticleLike,
                Message = message,
                CreatedAt = DateTime.UtcNow,
                IsRead = false,
                CulturalArticleId = articleId
            };

            await _notificationRepository.AddAsync(notification);
            await _unitOfWork.CompleteAsync();

            var notificationDto = new NotificationDto
            {
                Id = notification.Id,
                RecipientId = notification.RecipientId,
                ActorId = notification.ActorId,
                Actor = new UserPreviewDto
                {
                    Id = actor.Id,
                    FirstName = actor.FirstName,
                    LastName = actor.LastName,
                    ProfilePicture = actor.ProfilePicture
                },
                Type = notification.Type,
                Message = notification.Message,
                CreatedAt = notification.CreatedAt,
                IsRead = notification.IsRead,
                ArticleId = notification.CulturalArticleId
            };

            return new GenericResponseDto<NotificationDto>
            {
                Success = true,
                Message = "Article like notification created successfully",
                Data = notificationDto
            };
        }

        public async Task<GenericResponseDto<NotificationDto>> CreateArticleCommentNotificationAsync(int actorId, int articleId, string message)
        {
            var actor = await _userRepository.GetByIdAsync(actorId);
            if (actor == null)
            {
                return new GenericResponseDto<NotificationDto>
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            var article = await _articleRepository.GetByIdAsync(articleId);
            if (article == null)
            {
                return new GenericResponseDto<NotificationDto>
                {
                    Success = false,
                    Message = "Article not found"
                };
            }

            if (article.UserId == actorId)
            {
                return new GenericResponseDto<NotificationDto>
                {
                    Success = false,
                    Message = "Cannot create notification for self-comment"
                };
            }

            var notification = new Notification
            {
                ActorId = actorId,
                RecipientId = article.UserId,
                Type = NotificationType.ArticleComment,
                Message = message,
                CreatedAt = DateTime.UtcNow,
                IsRead = false,
                CulturalArticleId = articleId
            };

            await _notificationRepository.AddAsync(notification);
            await _unitOfWork.CompleteAsync();

            var notificationDto = new NotificationDto
            {
                Id = notification.Id,
                RecipientId = notification.RecipientId,
                ActorId = notification.ActorId,
                Actor = new UserPreviewDto
                {
                    Id = actor.Id,
                    FirstName = actor.FirstName,
                    LastName = actor.LastName,
                    ProfilePicture = actor.ProfilePicture
                },
                Type = notification.Type,
                Message = notification.Message,
                CreatedAt = notification.CreatedAt,
                IsRead = notification.IsRead,
                ArticleId = notification.CulturalArticleId
            };

            return new GenericResponseDto<NotificationDto>
            {
                Success = true,
                Message = "Article comment notification created successfully",
                Data = notificationDto
            };
        }

        public async Task<GenericResponseDto<NotificationDto>> CreateRepostNotificationAsync(int actorId, int recipientId, string message)
        {
            // Validate users exist
            var actor = await _userRepository.GetByIdAsync(actorId);
            var recipient = await _userRepository.GetByIdAsync(recipientId);

            if (actor == null || recipient == null)
            {
                return new GenericResponseDto<NotificationDto>
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            // Don't notify if user reposts their own article
            if (actorId == recipientId)
            {
                return new GenericResponseDto<NotificationDto>
                {
                    Success = false,
                    Message = "Cannot create notification for self-repost"
                };
            }

            var notification = new Notification
            {
                ActorId = actorId,
                RecipientId = recipientId,
                Type = NotificationType.Repost,
                Message = message,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            await _notificationRepository.AddAsync(notification);
            await _unitOfWork.CompleteAsync();

            var notificationDto = new NotificationDto
            {
                Id = notification.Id,
                RecipientId = notification.RecipientId,
                ActorId = notification.ActorId,
                Actor = new UserPreviewDto
                {
                    Id = actor.Id,
                    FirstName = actor.FirstName,
                    LastName = actor.LastName,
                    ProfilePicture = actor.ProfilePicture
                },
                Type = notification.Type,
                Message = notification.Message,
                CreatedAt = notification.CreatedAt,
                IsRead = notification.IsRead
            };

            return new GenericResponseDto<NotificationDto>
            {
                Success = true,
                Message = "Repost notification created successfully",
                Data = notificationDto
            };
        }



        public async Task<GenericResponseDto<PaginationResponse<NotificationDto>>> GetUnreadNotificationsAsync(int userId, int page = 1, int pageSize = 10)
        {
            var includes = new string[] { "Actor" };
            var notifications = await _notificationRepository
                .GetAllPredicated(n => n.RecipientId == userId && !n.IsRead, includes);

            var specParams = new SpecParams
            {
                PageIndex = page,
                PageSize = pageSize
            };


            var paginatedNotifications = _paginationService.Paginate(notifications, specParams, notification =>
            {
                return MapToDto(notification);
            });

            // Construct response
            return new GenericResponseDto<PaginationResponse<NotificationDto>>
            {
                Success = true,
                Message = paginatedNotifications.Items.Any()
                    ? "Unread notifications retrieved successfully"
                    : "No unread notifications found",
                Data = paginatedNotifications
            };
        }

        public async Task<ResponseDto> MarkNotificationAsReadAsync(int notificationId, int userId)
        {
            var notification = await _notificationRepository.FindAsync(n => n.Id == notificationId && n.RecipientId == userId);

            if (notification == null)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 404,
                    Message = "Notification not found or does not belong to the user"
                };
            }

            // Mark as read
            notification.IsRead = true;
            _notificationRepository.Update(notification);
            await _unitOfWork.CompleteAsync();

            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Message = "Notification marked as read successfully"
            };
        }

        public async Task<ResponseDto> MarkAllNotificationsAsReadAsync(int userId)
        {
            // Verify user exists
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 404,
                    Message = "User not found"
                };
            }

            // Get all unread notifications for this user
            var spec = new EntitySpecification<Notification>(
                new SpecParams(),
                n => n.RecipientId == userId && !n.IsRead);

            var notifications = await _notificationRepository.ListAsync(spec);

            if (!notifications.Any())
            {
                return new ResponseDto
                {
                    IsSucceeded = true,
                    Status = 200,
                    Message = "No unread notifications found"
                };
            }

            // Mark all as read
            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                _notificationRepository.Update(notification);
            }

            await _unitOfWork.CompleteAsync();

            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Message = $"{notifications.Count} notifications marked as read successfully"
            };
        }

        public async Task<GenericResponseDto<PaginationResponse<NotificationDto>>> GetNotificationsByPredicateAsync(
            Expression<Func<Notification, bool>> predicate,
            string[] includes = null,
            int page = 1,
            int pageSize = 10)
        {
            var notifications = await _notificationRepository.GetAllPredicated(predicate, includes);

            if (!notifications.Any())
            {
                return new GenericResponseDto<PaginationResponse<NotificationDto>>
                {
                    Success = true,
                    Message = "No notifications found matching criteria",
                    Data = new PaginationResponse<NotificationDto>
                    {
                        PageIndex = page,
                        PageSize = pageSize,
                        TotalItems = 0,
                        TotalPages = 0,
                        Items = new List<NotificationDto>()
                    }
                };
            }

            // Fetch actors
            var actorIds = notifications.Select(n => n.ActorId).Distinct().ToList();
            var actors = await _userRepository.GetAllPredicated(u => actorIds.Contains(u.Id), includes);

            // Apply pagination using your PaginationService
            var specParams = new SpecParams { PageIndex = page, PageSize = pageSize };
            var paginated = _paginationService.Paginate(notifications, specParams, notification =>
                MapToDto(notification, (List<User>)actors)
            );

            return new GenericResponseDto<PaginationResponse<NotificationDto>>
            {
                Success = true,
                Message = "Notifications retrieved successfully",
                Data = paginated
            };
        }



        public async Task<ResponseDto> GetNotificationStatsAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 404,
                    Message = "User not found"
                };
            }

            // Create specifications for counting
            var unreadSpec = new EntitySpecification<Notification>(new SpecParams(), n => n.RecipientId == userId && !n.IsRead);
            var totalSpec = new EntitySpecification<Notification>(new SpecParams(), n => n.RecipientId == userId);

            var unreadCount = await _notificationRepository.CountAsync(unreadSpec);
            var totalCount = await _notificationRepository.CountAsync(totalSpec);

            var stats = new NotificationStatsDto
            {
                UserId = userId,
                UnreadCount = unreadCount,
                TotalCount = totalCount
            };

            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Model = stats,
                Message = "Notification statistics retrieved successfully"
            };
        }



        private NotificationDto MapToDto(Notification notification, List<User> actors)
        {
            var actor = actors.FirstOrDefault(a => a.Id == notification.ActorId);
            return new NotificationDto
            {
                Id = notification.Id,
                RecipientId = notification.RecipientId,
                ActorId = notification.ActorId,
                Actor = actor != null ? new UserPreviewDto
                {
                    Id = actor.Id,
                    FirstName = actor.FirstName,
                    LastName = actor.LastName,
                    ProfilePicture = actor.ProfilePicture
                } : null,
                Type = notification.Type,
                Message = notification.Message,
                CreatedAt = notification.CreatedAt,
                IsRead = notification.IsRead,
                ArticleId = notification.CulturalArticleId,
                CommentId = notification.CommentId
            };
        }


        private NotificationDto MapToDto(Notification notification)
        {
            return new NotificationDto
            {
                Id = notification.Id,
                RecipientId = notification.RecipientId,
                ActorId = notification.ActorId,
                Actor = new UserPreviewDto
                {
                    Id = notification.Actor.Id,
                    FirstName = notification.Actor.FirstName,
                    LastName = notification.Actor.LastName,
                    ProfilePicture = notification.Actor.ProfilePicture
                },
                Type = notification.Type,
                Message = notification.Message,
                CreatedAt = notification.CreatedAt,
                IsRead = notification.IsRead
            };
        }


    }
}