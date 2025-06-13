using AutoMapper;
using OurHeritage.Core.Entities;
using OurHeritage.Core.Specifications;
using OurHeritage.Repo.Repositories.Interfaces;
using OurHeritage.Service.DTOs;
using OurHeritage.Service.DTOs.StatisticsDto;
using OurHeritage.Service.DTOs.UserDto;
using OurHeritage.Service.Interfaces;

namespace OurHeritage.Service.Implementations
{
    public class StatisticsService : IStatisticsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IPaginationService _paginationService;

        public StatisticsService(IUnitOfWork unitOfWork, IMapper mapper, IPaginationService paginationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _paginationService = paginationService;
        }

        public async Task<ResponseDto> GetAdminStatisticsAsync()
        {
            try
            {
                // Get all entities
                var users = await _unitOfWork.Repository<User>().ListAllAsync();
                var articles = await _unitOfWork.Repository<CulturalArticle>()
                    .GetAllPredicated(a => true, new[] { "User", "Category", "Likes", "Comments" });
                var handiCrafts = await _unitOfWork.Repository<HandiCraft>()
                    .GetAllPredicated(h => true, new[] { "User", "Category", "Favorite" });
                var likes = await _unitOfWork.Repository<Like>().ListAllAsync();
                var comments = await _unitOfWork.Repository<Comment>().ListAllAsync();
                var favorites = await _unitOfWork.Repository<Favorite>().ListAllAsync();
                var follows = await _unitOfWork.Repository<Follow>().ListAllAsync();
                var categories = await _unitOfWork.Repository<Category>().ListAllAsync();

                // Calculate basic statistics
                var now = DateTime.Now;
                var todayStart = now.Date;
                var monthStart = new DateTime(now.Year, now.Month, 1);

                var statistics = new AdminStatisticsDto
                {
                    TotalUsers = users.Count(),
                    TotalCulturalArticles = articles.Count(),
                    TotalHandiCrafts = handiCrafts.Count(),
                    TotalLikes = likes.Count(),
                    TotalComments = comments.Count(),
                    TotalFavorites = favorites.Count(),
                    TotalFollows = follows.Count(),
                    TotalCategories = categories.Count(),

                    // Active users today (users who posted, liked, or commented today)
                    ActiveUsersToday = GetActiveUsersToday(articles, handiCrafts, likes, comments, todayStart),

                    // New registrations this month
                    NewUsersThisMonth = users.Count(u => u.DateJoined >= monthStart),

                    // Content created this month
                    ArticlesThisMonth = articles.Count(a => a.DateCreated >= monthStart),
                    HandiCraftsThisMonth = handiCrafts.Count(h => h.DateAdded >= monthStart),

                    // Category statistics (top 5 for summary)
                    CategoryStats = GetCategoryStatistics(articles, handiCrafts, categories).Take(5).ToList(),

                    // Monthly activity for the last 12 months
                    MonthlyActivity = GetMonthlyActivity(users, articles, handiCrafts, likes, comments),

                    // Top active users (top 5 for summary)
                    TopActiveUsers = GetTopActiveUsers(users, articles, handiCrafts, likes, follows).Take(5).ToList(),

                    // Popular content (top 3 for summary)
                    PopularArticles = GetPopularArticles(articles).Take(3).ToList(),
                    PopularHandiCrafts = GetPopularHandiCrafts(handiCrafts).Take(3).ToList()
                };

                return new ResponseDto
                {
                    IsSucceeded = true,
                    Status = 200,
                    Model = statistics,
                    Message = "Admin statistics retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 500,
                    Message = $"Error retrieving statistics: {ex.Message}"
                };
            }
        }

        // Paginated method for top active users using PaginationService
        public async Task<ResponseDto> GetTopActiveUsersPaginatedAsync(SpecParams specParams)
        {
            try
            {
                var users = await _unitOfWork.Repository<User>().ListAllAsync();
                var articles = await _unitOfWork.Repository<CulturalArticle>()
                    .GetAllPredicated(a => true, new[] { "Likes" });
                var handiCrafts = await _unitOfWork.Repository<HandiCraft>().ListAllAsync();
                var follows = await _unitOfWork.Repository<Follow>().ListAllAsync();

                var topUsers = GetTopActiveUsers(users, articles, handiCrafts, new List<Like>(), follows);

                var paginatedUsers = _paginationService.Paginate(topUsers, specParams, user => user);

                return new ResponseDto
                {
                    IsSucceeded = true,
                    Status = 200,
                    Model = paginatedUsers,
                    Message = "Top active users retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 500,
                    Message = $"Error retrieving top active users: {ex.Message}"
                };
            }
        }

        // Paginated method for popular articles using PaginationService
        public async Task<ResponseDto> GetPopularArticlesPaginatedAsync(SpecParams specParams)
        {
            try
            {
                var articles = await _unitOfWork.Repository<CulturalArticle>()
                    .GetAllPredicated(a => true, new[] { "User", "Likes", "Comments" });

                var popularArticles = GetPopularArticles(articles);
                var paginatedArticles = _paginationService.Paginate(popularArticles, specParams, article => article);

                return new ResponseDto
                {
                    IsSucceeded = true,
                    Status = 200,
                    Model = paginatedArticles,
                    Message = "Popular articles retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 500,
                    Message = $"Error retrieving popular articles: {ex.Message}"
                };
            }
        }

        // Paginated method for popular handicrafts using PaginationService
        public async Task<ResponseDto> GetPopularHandiCraftsPaginatedAsync(SpecParams specParams)
        {
            try
            {
                var handiCrafts = await _unitOfWork.Repository<HandiCraft>()
                    .GetAllPredicated(h => true, new[] { "User", "Favorite" });

                var popularHandiCrafts = GetPopularHandiCrafts(handiCrafts);
                var paginatedHandiCrafts = _paginationService.Paginate(popularHandiCrafts, specParams, handicraft => handicraft);

                return new ResponseDto
                {
                    IsSucceeded = true,
                    Status = 200,
                    Model = paginatedHandiCrafts,
                    Message = "Popular handicrafts retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 500,
                    Message = $"Error retrieving popular handicrafts: {ex.Message}"
                };
            }
        }

        // Enhanced category statistics with pagination using PaginationService
        public async Task<ResponseDto> GetCategoryStatisticsPaginatedAsync(SpecParams specParams)
        {
            try
            {
                var categories = await _unitOfWork.Repository<Category>().ListAllAsync();
                var articles = await _unitOfWork.Repository<CulturalArticle>().ListAllAsync();
                var handiCrafts = await _unitOfWork.Repository<HandiCraft>().ListAllAsync();

                var categoryStats = GetCategoryStatistics(articles, handiCrafts, categories);
                var paginatedStats = _paginationService.Paginate(categoryStats, specParams, stat => stat);

                return new ResponseDto
                {
                    IsSucceeded = true,
                    Status = 200,
                    Model = paginatedStats,
                    Message = "Category statistics retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 500,
                    Message = $"Error retrieving category statistics: {ex.Message}"
                };
            }
        }

        // New method for user activity history with pagination using PaginationService
        public async Task<ResponseDto> GetUserActivityHistoryAsync(int userId, SpecParams specParams)
        {
            try
            {
                var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
                if (user == null)
                {
                    return new ResponseDto
                    {
                        IsSucceeded = false,
                        Status = 404,
                        Message = "User not found"
                    };
                }

                var articles = await _unitOfWork.Repository<CulturalArticle>()
                    .GetAllPredicated(a => a.UserId == userId, new[] { "Likes", "Comments" });
                var handiCrafts = await _unitOfWork.Repository<HandiCraft>()
                    .GetAllPredicated(h => h.UserId == userId, new[] { "Favorite" });

                // Combine activities and sort by date
                var activities = new List<UserActivityDto>();

                activities.AddRange(articles.Select(a => new UserActivityDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    Type = "Article",
                    Date = a.DateCreated,
                    Engagement = (a.Likes?.Count ?? 0) + (a.Comments?.Count ?? 0)
                }));

                activities.AddRange(handiCrafts.Select(h => new UserActivityDto
                {
                    Id = h.Id,
                    Title = h.Title,
                    Type = "HandiCraft",
                    Date = h.DateAdded,
                    Engagement = h.Favorite?.Count ?? 0
                }));

                var sortedActivities = activities.OrderByDescending(a => a.Date);
                var paginatedActivities = _paginationService.Paginate(sortedActivities, specParams, activity => activity);

                return new ResponseDto
                {
                    IsSucceeded = true,
                    Status = 200,
                    Model = paginatedActivities,
                    Message = "User activity history retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 500,
                    Message = $"Error retrieving user activity history: {ex.Message}"
                };
            }
        }

        // Method to get all users with pagination for admin management
        public async Task<ResponseDto> GetAllUsersPaginatedAsync(SpecParams specParams)
        {
            try
            {
                var users = await _unitOfWork.Repository<User>().ListAllAsync();
                var articles = await _unitOfWork.Repository<CulturalArticle>().ListAllAsync();
                var handiCrafts = await _unitOfWork.Repository<HandiCraft>().ListAllAsync();
                var follows = await _unitOfWork.Repository<Follow>().ListAllAsync();

                var userDtos = users.Select(u => new UserStatisticsDto
                {
                    UserId = u.Id,
                    UserName = $"{u.FirstName} {u.LastName}",
                    Email = u.Email,
                    ProfilePicture = u.ProfilePicture ?? "default.jpg",
                    DateJoined = u.DateJoined,
                    ArticleCount = articles.Count(a => a.UserId == u.Id),
                    HandiCraftCount = handiCrafts.Count(h => h.UserId == u.Id),
                    FollowersCount = follows.Count(f => f.FollowingId == u.Id),
                    FollowingCount = follows.Count(f => f.FollowerId == u.Id),
                    IsActive = u.DateJoined >= DateTime.Now.AddDays(-30) // Active in last 30 days
                }).OrderByDescending(u => u.DateJoined);

                var paginatedUsers = _paginationService.Paginate(userDtos, specParams, user => user);

                return new ResponseDto
                {
                    IsSucceeded = true,
                    Status = 200,
                    Model = paginatedUsers,
                    Message = "Users retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 500,
                    Message = $"Error retrieving users: {ex.Message}"
                };
            }
        }

        // Method to get content engagement with pagination
        public async Task<ResponseDto> GetContentEngagementPaginatedAsync(SpecParams specParams, string contentType = "all")
        {
            try
            {
                var engagementItems = new List<ContentEngagementDto>();

                if (contentType == "all" || contentType == "articles")
                {
                    var articles = await _unitOfWork.Repository<CulturalArticle>()
                        .GetAllPredicated(a => true, new[] { "User", "Likes", "Comments" });

                    engagementItems.AddRange(articles.Select(a => new ContentEngagementDto
                    {
                        Id = a.Id,
                        Title = a.Title,
                        ContentType = "Article",
                        CreatorName = a.User != null ? $"{a.User.FirstName} {a.User.LastName}" : "Unknown",
                        DateCreated = a.DateCreated,
                        LikeCount = a.Likes?.Count ?? 0,
                        CommentCount = a.Comments?.Count ?? 0,
                        FavoriteCount = 0,
                        TotalEngagement = (a.Likes?.Count ?? 0) + (a.Comments?.Count ?? 0)
                    }));
                }

                if (contentType == "all" || contentType == "handicrafts")
                {
                    var handiCrafts = await _unitOfWork.Repository<HandiCraft>()
                        .GetAllPredicated(h => true, new[] { "User", "Favorite" });

                    engagementItems.AddRange(handiCrafts.Select(h => new ContentEngagementDto
                    {
                        Id = h.Id,
                        Title = h.Title,
                        ContentType = "HandiCraft",
                        CreatorName = h.User != null ? $"{h.User.FirstName} {h.User.LastName}" : "Unknown",
                        DateCreated = h.DateAdded,
                        LikeCount = 0,
                        CommentCount = 0,
                        FavoriteCount = h.Favorite?.Count ?? 0,
                        TotalEngagement = h.Favorite?.Count ?? 0
                    }));
                }

                var sortedContent = engagementItems.OrderByDescending(c => c.TotalEngagement);
                var paginatedContent = _paginationService.Paginate(sortedContent, specParams, content => content);

                return new ResponseDto
                {
                    IsSucceeded = true,
                    Status = 200,
                    Model = paginatedContent,
                    Message = "Content engagement retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 500,
                    Message = $"Error retrieving content engagement: {ex.Message}"
                };
            }
        }

        // Keep original methods for backward compatibility
        public async Task<ResponseDto> GetUserStatisticsAsync(int userId)
        {
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
            if (user == null)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 404,
                    Message = "User not found"
                };
            }

            var articles = await _unitOfWork.Repository<CulturalArticle>()
                .GetAllPredicated(a => a.UserId == userId, new[] { "Likes", "Comments" });
            var handiCrafts = await _unitOfWork.Repository<HandiCraft>()
                .GetAllPredicated(h => h.UserId == userId, new[] { "Favorite" });
            var followers = await _unitOfWork.Repository<Follow>()
                .GetAllPredicated(f => f.FollowingId == userId, null);
            var following = await _unitOfWork.Repository<Follow>()
                .GetAllPredicated(f => f.FollowerId == userId, null);

            var userStats = new
            {
                UserId = userId,
                UserName = $"{user.FirstName} {user.LastName}",
                TotalArticles = articles.Count(),
                TotalHandiCrafts = handiCrafts.Count(),
                TotalLikesReceived = articles.Sum(a => a.Likes?.Count ?? 0),
                TotalCommentsReceived = articles.Sum(a => a.Comments?.Count ?? 0),
                TotalFavoritesReceived = handiCrafts.Sum(h => h.Favorite?.Count ?? 0),
                FollowersCount = followers.Count(),
                FollowingCount = following.Count(),
                MemberSince = user.DateJoined.ToString("yyyy-MM-dd")
            };

            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Model = userStats,
                Message = "User statistics retrieved successfully"
            };
        }

        public async Task<ResponseDto> GetMonthlyReportAsync(int year, int month)
        {
            var monthStart = new DateTime(year, month, 1);
            var monthEnd = monthStart.AddMonths(1);

            var users = await _unitOfWork.Repository<User>()
                .GetAllPredicated(u => u.DateJoined >= monthStart && u.DateJoined < monthEnd, null);
            var articles = await _unitOfWork.Repository<CulturalArticle>()
                .GetAllPredicated(a => a.DateCreated >= monthStart && a.DateCreated < monthEnd, new[] { "Likes", "Comments" });
            var handiCrafts = await _unitOfWork.Repository<HandiCraft>()
                .GetAllPredicated(h => h.DateAdded >= monthStart && h.DateAdded < monthEnd, new[] { "Favorite" });

            var monthlyReport = new
            {
                Year = year,
                Month = month,
                MonthName = monthStart.ToString("MMMM"),
                NewUsers = users.Count(),
                NewArticles = articles.Count(),
                NewHandiCrafts = handiCrafts.Count(),
                TotalEngagement = articles.Sum(a => (a.Likes?.Count ?? 0) + (a.Comments?.Count ?? 0)) +
                                 handiCrafts.Sum(h => h.Favorite?.Count ?? 0)
            };

            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Model = monthlyReport,
                Message = "Monthly report retrieved successfully"
            };
        }

        public async Task<ResponseDto> GetCategoryStatisticsAsync()
        {
            var categories = await _unitOfWork.Repository<Category>().ListAllAsync();
            var articles = await _unitOfWork.Repository<CulturalArticle>().ListAllAsync();
            var handiCrafts = await _unitOfWork.Repository<HandiCraft>().ListAllAsync();

            var categoryStats = categories.Select(c => new CategoryStatDto
            {
                CategoryId = c.Id,
                CategoryName = c.Name,
                ArticleCount = articles.Count(a => a.CategoryId == c.Id),
                HandiCraftCount = handiCrafts.Count(h => h.CategoryId == c.Id),
                TotalCount = articles.Count(a => a.CategoryId == c.Id) + handiCrafts.Count(h => h.CategoryId == c.Id)
            }).OrderByDescending(cs => cs.TotalCount).ToList();

            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Models = categoryStats,
                Message = "Category statistics retrieved successfully"
            };
        }

        public async Task<ResponseDto> GetContentEngagementStatsAsync()
        {
            var articles = await _unitOfWork.Repository<CulturalArticle>()
                .GetAllPredicated(a => true, new[] { "Likes", "Comments" });
            var handiCrafts = await _unitOfWork.Repository<HandiCraft>()
                .GetAllPredicated(h => true, new[] { "Favorite" });

            var engagementStats = new
            {
                AverageArticleLikes = articles.Any() ? articles.Average(a => a.Likes?.Count ?? 0) : 0,
                AverageArticleComments = articles.Any() ? articles.Average(a => a.Comments?.Count ?? 0) : 0,
                AverageHandiCraftFavorites = handiCrafts.Any() ? handiCrafts.Average(h => h.Favorite?.Count ?? 0) : 0,
                MostLikedArticle = articles.OrderByDescending(a => a.Likes?.Count ?? 0).FirstOrDefault(),
                MostCommentedArticle = articles.OrderByDescending(a => a.Comments?.Count ?? 0).FirstOrDefault(),
                MostFavoritedHandiCraft = handiCrafts.OrderByDescending(h => h.Favorite?.Count ?? 0).FirstOrDefault()
            };

            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Model = engagementStats,
                Message = "Content engagement statistics retrieved successfully"
            };
        }

        #region Helper Methods

        private int GetActiveUsersToday(IEnumerable<CulturalArticle> articles, IEnumerable<HandiCraft> handiCrafts,
            IEnumerable<Like> likes, IEnumerable<Comment> comments, DateTime todayStart)
        {
            var activeUserIds = new HashSet<int>();

            // Users who posted articles today
            activeUserIds.UnionWith(articles.Where(a => a.DateCreated >= todayStart).Select(a => a.UserId));

            // Users who posted handicrafts today
            activeUserIds.UnionWith(handiCrafts.Where(h => h.DateAdded >= todayStart).Select(h => h.UserId));

            // Users who liked something today
            activeUserIds.UnionWith(likes.Where(l => l.LikedAt >= todayStart).Select(l => l.UserId));

            // Users who commented today
            activeUserIds.UnionWith(comments.Where(c => c.DateCreated >= todayStart).Select(c => c.UserId));

            return activeUserIds.Count;
        }

        private List<CategoryStatDto> GetCategoryStatistics(IEnumerable<CulturalArticle> articles,
            IEnumerable<HandiCraft> handiCrafts, IEnumerable<Category> categories)
        {
            return categories.Select(c => new CategoryStatDto
            {
                CategoryId = c.Id,
                CategoryName = c.Name,
                ArticleCount = articles.Count(a => a.CategoryId == c.Id),
                HandiCraftCount = handiCrafts.Count(h => h.CategoryId == c.Id),
                TotalCount = articles.Count(a => a.CategoryId == c.Id) + handiCrafts.Count(h => h.CategoryId == c.Id)
            }).OrderByDescending(cs => cs.TotalCount).ToList();
        }

        private List<MonthlyActivityDto> GetMonthlyActivity(IEnumerable<User> users, IEnumerable<CulturalArticle> articles,
            IEnumerable<HandiCraft> handiCrafts, IEnumerable<Like> likes, IEnumerable<Comment> comments)
        {
            var monthlyActivity = new List<MonthlyActivityDto>();
            var now = DateTime.Now;

            for (int i = 11; i >= 0; i--)
            {
                var targetDate = now.AddMonths(-i);
                var monthStart = new DateTime(targetDate.Year, targetDate.Month, 1);
                var monthEnd = monthStart.AddMonths(1);

                monthlyActivity.Add(new MonthlyActivityDto
                {
                    Month = targetDate.ToString("MMM"),
                    Year = targetDate.Year,
                    NewUsers = users.Count(u => u.DateJoined >= monthStart && u.DateJoined < monthEnd),
                    NewArticles = articles.Count(a => a.DateCreated >= monthStart && a.DateCreated < monthEnd),
                    NewHandiCrafts = handiCrafts.Count(h => h.DateAdded >= monthStart && h.DateAdded < monthEnd),
                    TotalLikes = likes.Count(l => l.LikedAt >= monthStart && l.LikedAt < monthEnd),
                    TotalComments = comments.Count(c => c.DateCreated >= monthStart && c.DateCreated < monthEnd)
                });
            }

            return monthlyActivity;
        }

        private List<TopUserDto> GetTopActiveUsers(IEnumerable<User> users, IEnumerable<CulturalArticle> articles,
            IEnumerable<HandiCraft> handiCrafts, IEnumerable<Like> likes, IEnumerable<Follow> follows)
        {
            return users.Select(u => new TopUserDto
            {
                UserId = u.Id,
                UserName = $"{u.FirstName} {u.LastName}",
                ProfilePicture = u.ProfilePicture ?? "default.jpg",
                ArticleCount = articles.Count(a => a.UserId == u.Id),
                HandiCraftCount = handiCrafts.Count(h => h.UserId == u.Id),
                TotalLikes = articles.Where(a => a.UserId == u.Id).Sum(a => a.Likes?.Count ?? 0),
                FollowersCount = follows.Count(f => f.FollowingId == u.Id),
                ActivityScore = articles.Count(a => a.UserId == u.Id) * 3 +
                              handiCrafts.Count(h => h.UserId == u.Id) * 3 +
                              articles.Where(a => a.UserId == u.Id).Sum(a => a.Likes?.Count ?? 0) +
                              follows.Count(f => f.FollowingId == u.Id) * 2
            })
            .OrderByDescending(u => u.ActivityScore)
            .ToList();
        }

        private List<PopularContentDto> GetPopularArticles(IEnumerable<CulturalArticle> articles)
        {
            return articles
                .OrderByDescending(a => (a.Likes?.Count ?? 0) + (a.Comments?.Count ?? 0))
                .Select(a => new PopularContentDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    CreatorName = a.User != null ? $"{a.User.FirstName} {a.User.LastName}" : "Unknown",
                    LikeCount = a.Likes?.Count ?? 0,
                    CommentCount = a.Comments?.Count ?? 0,
                    FavoriteCount = 0,
                    DateCreated = a.DateCreated,
                    ContentType = "Article"
                })
                .ToList();
        }

        private List<PopularContentDto> GetPopularHandiCrafts(IEnumerable<HandiCraft> handiCrafts)
        {
            return handiCrafts
                .OrderByDescending(h => h.Favorite?.Count ?? 0)
                .Select(h => new PopularContentDto
                {
                    Id = h.Id,
                    Title = h.Title,
                    CreatorName = h.User != null ? $"{h.User.FirstName} {h.User.LastName}" : "Unknown",
                    LikeCount = 0,
                    CommentCount = 0,
                    FavoriteCount = h.Favorite?.Count ?? 0,
                    DateCreated = h.DateAdded,
                    ContentType = "HandiCraft"
                })
                .ToList();
        }

        #endregion
    }
}