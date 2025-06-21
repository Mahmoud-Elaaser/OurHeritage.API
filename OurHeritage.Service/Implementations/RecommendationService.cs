using Microsoft.EntityFrameworkCore;
using OurHeritage.Core.Context;
using OurHeritage.Core.Entities;
using OurHeritage.Core.Enums;
using OurHeritage.Service.DTOs.RecommendationSystemDto;
using OurHeritage.Service.Interfaces;

namespace OurHeritage.Service.Implementations
{
    public class RecommendationService : IRecommendationService
    {
        private readonly ApplicationDbContext _context;

        public RecommendationService(ApplicationDbContext context)
        {
            _context = context;
        }


        public async Task<RecommendationResponseDto> GetRecommendationsAsync(
            int userId,
            int pageSize = 10,
            int pageNumber = 1,
            RecommendationType filterType = RecommendationType.Both)
        {
            var userEngagement = await GetUserEngagementDataAsync(userId);

            if (!userEngagement.LikedArticleIds.Any() &&
                !userEngagement.CommentedArticleIds.Any() &&
                !userEngagement.FavoriteHandicraftIds.Any())
            {
                return await GetPopularItemsRecommendationsAsync(pageSize, pageNumber, filterType);
            }

            var allArticles = new List<RecommendationDto>();
            var allHandicrafts = new List<RecommendationDto>();

            // Only get articles if requested
            if (filterType == RecommendationType.Both || filterType == RecommendationType.ArticlesOnly)
            {
                var recommendedArticles = await GetContentBasedArticleRecommendationsAsync(userEngagement, pageSize / 2);
                var collaborativeArticles = await GetCollaborativeArticleRecommendationsAsync(userId, pageSize / 2);
                allArticles = CombineAndRankRecommendations(recommendedArticles, collaborativeArticles);
            }

            // Only get handicrafts if requested
            if (filterType == RecommendationType.Both || filterType == RecommendationType.HandicraftsOnly)
            {
                var recommendedHandicrafts = await GetContentBasedHandicraftRecommendationsAsync(userEngagement, pageSize / 2);
                var collaborativeHandicrafts = await GetCollaborativeHandicraftRecommendationsAsync(userId, pageSize / 2);
                allHandicrafts = CombineAndRankRecommendations(recommendedHandicrafts, collaborativeHandicrafts);
            }

            // Adjust the take count based on filter type
            var articlesToTake = filterType switch
            {
                RecommendationType.ArticlesOnly => pageSize,
                RecommendationType.HandicraftsOnly => 0,
                _ => pageSize / 2
            };

            var handicraftsToTake = filterType switch
            {
                RecommendationType.HandicraftsOnly => pageSize,
                RecommendationType.ArticlesOnly => 0,
                _ => pageSize / 2
            };

            return new RecommendationResponseDto
            {
                RecommendedArticles = allArticles.Take(articlesToTake).ToList(),
                RecommendedHandicrafts = allHandicrafts.Take(handicraftsToTake).ToList(),
                TotalCount = allArticles.Take(articlesToTake).Count() + allHandicrafts.Take(handicraftsToTake).Count(),
                RecommendationReason = GetRecommendationReason(filterType)
            };
        }

        // Add overload for backward compatibility
        public async Task<RecommendationResponseDto> GetRecommendationsAsync(int userId, int pageSize = 10, int pageNumber = 1)
        {
            return await GetRecommendationsAsync(userId, pageSize, pageNumber, RecommendationType.Both);
        }


        private async Task<RecommendationResponseDto> GetPopularItemsRecommendationsAsync(
            int pageSize,
            int pageNumber,
            RecommendationType filterType)
        {
            var popularArticles = new List<RecommendationDto>();
            var popularHandicrafts = new List<RecommendationDto>();

            // Only get articles if requested
            if (filterType == RecommendationType.Both || filterType == RecommendationType.ArticlesOnly)
            {
                var articlesToTake = filterType == RecommendationType.ArticlesOnly ? pageSize : pageSize / 2;

                popularArticles = await _context.Set<CulturalArticle>()
                    .Include(a => a.Category)
                    .Include(a => a.User)
                    .OrderByDescending(a => a.Likes.Count())
                    .ThenByDescending(a => a.Comments.Count())
                    .Skip((pageNumber - 1) * articlesToTake)
                    .Take(articlesToTake)
                    .Select(a => new RecommendationDto
                    {
                        Type = "Article",
                        ItemId = a.Id,
                        Title = a.Title,
                        Images = a.ImageURL ?? new List<string>(),
                        Content = a.Content.Length > 200 ? a.Content.Substring(0, 200) + "..." : a.Content,
                        CategoryName = a.Category.Name,
                        Creator = new CreatorDto
                        {
                            UserId = a.User.Id,
                            FullName = $"{a.User.FirstName} {a.User.LastName}",
                            Email = a.User.Email,
                            ProfilePicture = a.User.ProfilePicture,
                            Connections = a.User.Connections ?? new List<string>()
                        },
                        RecommendationScore = CalculateArticleScore(a)
                    })
                    .ToListAsync();
            }

            // Only get handicrafts if requested
            if (filterType == RecommendationType.Both || filterType == RecommendationType.HandicraftsOnly)
            {
                var handicraftsToTake = filterType == RecommendationType.HandicraftsOnly ? pageSize : pageSize / 2;

                popularHandicrafts = await _context.Set<HandiCraft>()
                    .Include(h => h.Category)
                    .Include(h => h.User)
                    .OrderByDescending(h => h.Favorite.Count())
                    .Skip((pageNumber - 1) * handicraftsToTake)
                    .Take(handicraftsToTake)
                    .Select(h => new RecommendationDto
                    {
                        Type = "Handicraft",
                        ItemId = h.Id,
                        Title = h.Title,
                        Images = h.ImageOrVideo ?? new List<string>(),
                        Content = h.Description.Length > 200 ? h.Description.Substring(0, 200) + "..." : h.Description,
                        CategoryName = h.Category.Name,
                        Price = h.Price,
                        Creator = new CreatorDto
                        {
                            UserId = h.User.Id,
                            FullName = $"{h.User.FirstName} {h.User.LastName}",
                            Email = h.User.Email,
                            ProfilePicture = h.User.ProfilePicture,
                            Connections = h.User.Connections ?? new List<string>()
                        },
                        RecommendationScore = CalculateHandicraftScore(h)
                    })
                    .ToListAsync();
            }

            return new RecommendationResponseDto
            {
                RecommendedArticles = popularArticles,
                RecommendedHandicrafts = popularHandicrafts,
                TotalCount = popularArticles.Count + popularHandicrafts.Count,
                RecommendationReason = GetPopularRecommendationReason(filterType)
            };
        }


        private static string GetRecommendationReason(RecommendationType filterType)
        {
            return filterType switch
            {
                RecommendationType.ArticlesOnly => "Based on your interactions with cultural articles and similar users",
                RecommendationType.HandicraftsOnly => "Based on your interactions with handicrafts and similar users",
                _ => "Based on your interactions and similar users"
            };
        }


        private static string GetPopularRecommendationReason(RecommendationType filterType)
        {
            return filterType switch
            {
                RecommendationType.ArticlesOnly => "Popular cultural articles trending now",
                RecommendationType.HandicraftsOnly => "Popular handicrafts trending now",
                _ => "Popular items trending now"
            };
        }


        public async Task<RecommendationResponseDto> GetRecommendationsByCategoryAsync(
            int userId,
            int categoryId,
            RecommendationType filterType = RecommendationType.Both,
            int pageSize = 10,
            int pageNumber = 1)
        {
            var allArticles = new List<RecommendationDto>();
            var allHandicrafts = new List<RecommendationDto>();

            // Get articles by category if requested
            if (filterType == RecommendationType.Both || filterType == RecommendationType.ArticlesOnly)
            {
                allArticles = await _context.Set<CulturalArticle>()
                    .Include(a => a.Category)
                    .Include(a => a.User)
                    .Where(a => a.CategoryId == categoryId &&
                               !_context.Set<Like>().Any(l => l.UserId == userId && l.CulturalArticleId == a.Id))
                    .OrderByDescending(a => a.Likes.Count())
                    .ThenByDescending(a => a.Comments.Count())
                    .Select(a => new RecommendationDto
                    {
                        Type = "Article",
                        ItemId = a.Id,
                        Title = a.Title,
                        Images = a.ImageURL ?? new List<string>(),
                        Content = a.Content.Length > 200 ? a.Content.Substring(0, 200) + "..." : a.Content,
                        CategoryName = a.Category.Name,
                        Creator = new CreatorDto
                        {
                            UserId = a.User.Id,
                            FullName = $"{a.User.FirstName} {a.User.LastName}",
                            Email = a.User.Email,
                            ProfilePicture = a.User.ProfilePicture,
                            Connections = a.User.Connections ?? new List<string>()
                        },
                        RecommendationScore = CalculateArticleScore(a)
                    })
                    .ToListAsync();
            }

            // Get handicrafts by category if requested
            if (filterType == RecommendationType.Both || filterType == RecommendationType.HandicraftsOnly)
            {
                allHandicrafts = await _context.Set<HandiCraft>()
                    .Include(h => h.Category)
                    .Include(h => h.User)
                    .Where(h => h.CategoryId == categoryId &&
                               !_context.Set<Favorite>().Any(f => f.UserId == userId && f.HandiCraftId == h.Id))
                    .OrderByDescending(h => h.Favorite.Count())
                    .Select(h => new RecommendationDto
                    {
                        Type = "Handicraft",
                        ItemId = h.Id,
                        Title = h.Title,
                        Images = h.ImageOrVideo ?? new List<string>(),
                        Content = h.Description.Length > 200 ? h.Description.Substring(0, 200) + "..." : h.Description,
                        CategoryName = h.Category.Name,
                        Price = h.Price,
                        Creator = new CreatorDto
                        {
                            UserId = h.User.Id,
                            FullName = $"{h.User.FirstName} {h.User.LastName}",
                            Email = h.User.Email,
                            ProfilePicture = h.User.ProfilePicture,
                            Connections = h.User.Connections ?? new List<string>()
                        },
                        RecommendationScore = CalculateHandicraftScore(h)
                    })
                    .ToListAsync();
            }

            // Apply pagination and filtering
            var articlesToTake = filterType switch
            {
                RecommendationType.ArticlesOnly => pageSize,
                RecommendationType.HandicraftsOnly => 0,
                _ => pageSize / 2
            };

            var handicraftsToTake = filterType switch
            {
                RecommendationType.HandicraftsOnly => pageSize,
                RecommendationType.ArticlesOnly => 0,
                _ => pageSize / 2
            };

            var pagedArticles = allArticles
                .Skip((pageNumber - 1) * articlesToTake)
                .Take(articlesToTake)
                .ToList();

            var pagedHandicrafts = allHandicrafts
                .Skip((pageNumber - 1) * handicraftsToTake)
                .Take(handicraftsToTake)
                .ToList();

            return new RecommendationResponseDto
            {
                RecommendedArticles = pagedArticles,
                RecommendedHandicrafts = pagedHandicrafts,
                TotalCount = pagedArticles.Count + pagedHandicrafts.Count,
                RecommendationReason = $"Items from selected category - {GetRecommendationReason(filterType)}"
            };
        }


        public async Task<UserEngagementDto> GetUserEngagementDataAsync(int userId)
        {
            var likes = await _context.Set<Like>()
                .Where(l => l.UserId == userId)
                .Select(l => l.CulturalArticleId)
                .ToListAsync();

            var comments = await _context.Set<Comment>()
                .Where(c => c.UserId == userId)
                .Select(c => c.CulturalArticleId)
                .ToListAsync();

            var favorites = await _context.Set<Favorite>()
                .Where(f => f.UserId == userId)
                .Select(f => f.HandiCraftId)
                .ToListAsync();

            // Get engaged categories
            var articleCategoryIds = await _context.Set<CulturalArticle>()
                .Where(a => likes.Contains(a.Id) || comments.Contains(a.Id))
                .Select(a => a.CategoryId)
                .ToListAsync();

            var handicraftCategoryIds = await _context.Set<HandiCraft>()
                .Where(h => favorites.Contains(h.Id))
                .Select(h => h.CategoryId)
                .ToListAsync();

            var engagedCategories = articleCategoryIds.Concat(handicraftCategoryIds).Distinct().ToList();

            return new UserEngagementDto
            {
                UserId = userId,
                LikedArticleIds = likes,
                CommentedArticleIds = comments,
                FavoriteHandicraftIds = favorites,
                EngagedCategoryIds = engagedCategories
            };
        }

        private async Task<List<RecommendationDto>> GetContentBasedArticleRecommendationsAsync(UserEngagementDto engagement, int count)
        {
            var engagedArticleIds = engagement.LikedArticleIds.Concat(engagement.CommentedArticleIds).Distinct();

            var recommendations = await _context.Set<CulturalArticle>()
                .Include(a => a.Category)
                .Include(a => a.User)
                .Where(a => !engagedArticleIds.Contains(a.Id) &&
                           engagement.EngagedCategoryIds.Contains(a.CategoryId))
                .OrderByDescending(a => a.Likes.Count())
                .ThenByDescending(a => a.Comments.Count())
                .Take(count)
                .Select(a => new RecommendationDto
                {
                    Type = "Article",
                    ItemId = a.Id,
                    Title = a.Title,
                    Images = a.ImageURL ?? new List<string>(),
                    Content = a.Content.Length > 200 ? a.Content.Substring(0, 200) + "..." : a.Content,
                    CategoryName = a.Category.Name,
                    Creator = new CreatorDto
                    {
                        UserId = a.User.Id,
                        FullName = $"{a.User.FirstName} {a.User.LastName}",
                        Email = a.User.Email,
                        ProfilePicture = a.User.ProfilePicture,
                        Connections = a.User.Connections ?? new List<string>()
                    },
                    RecommendationScore = CalculateArticleScore(a)
                })
                .ToListAsync();

            return recommendations;
        }

        private async Task<List<RecommendationDto>> GetContentBasedHandicraftRecommendationsAsync(UserEngagementDto engagement, int count)
        {
            var recommendations = await _context.Set<HandiCraft>()
                .Include(h => h.Category)
                .Include(h => h.User)
                .Where(h => !engagement.FavoriteHandicraftIds.Contains(h.Id) &&
                           engagement.EngagedCategoryIds.Contains(h.CategoryId))
                .OrderByDescending(h => h.Favorite.Count())
                .Take(count)
                .Select(h => new RecommendationDto
                {
                    Type = "Handicraft",
                    ItemId = h.Id,
                    Title = h.Title,
                    Images = h.ImageOrVideo ?? new List<string>(),
                    Content = h.Description.Length > 200 ? h.Description.Substring(0, 200) + "..." : h.Description,
                    CategoryName = h.Category.Name,
                    Price = h.Price,
                    Creator = new CreatorDto
                    {
                        UserId = h.User.Id,
                        FullName = $"{h.User.FirstName} {h.User.LastName}",
                        Email = h.User.Email,
                        ProfilePicture = h.User.ProfilePicture,
                        Connections = h.User.Connections ?? new List<string>()
                    },
                    RecommendationScore = CalculateHandicraftScore(h)
                })
                .ToListAsync();

            return recommendations;
        }

        private async Task<List<RecommendationDto>> GetCollaborativeArticleRecommendationsAsync(int userId, int count)
        {
            // Find users with similar engagement patterns
            var similarUsers = await FindSimilarUsersAsync(userId);

            var rawArticles = await _context.Set<Like>()
                .Include(l => l.CulturalArticle).ThenInclude(a => a.Category)
                .Include(l => l.CulturalArticle).ThenInclude(a => a.User)
                .Where(l => similarUsers.Contains(l.UserId) && l.UserId != userId)
                .Select(l => l.CulturalArticle)
                .Where(a => !_context.Set<Like>().Any(ul => ul.UserId == userId && ul.CulturalArticleId == a.Id))
                .GroupBy(a => a.Id)
                .OrderByDescending(g => g.Count())
                .Take(count)
                .Select(g => g.First())
                .ToListAsync();

            var recommendations = rawArticles.Select(a => new RecommendationDto
            {
                Type = "Article",
                ItemId = a.Id,
                Title = a.Title,
                Images = a.ImageURL ?? new List<string>(),
                Content = a.Content.Length > 200 ? a.Content.Substring(0, 200) + "..." : a.Content,
                CategoryName = a.Category?.Name,
                Creator = new CreatorDto
                {
                    UserId = a.User?.Id ?? 0,
                    FullName = $"{a.User?.FirstName} {a.User?.LastName}",
                    Email = a.User?.Email,
                    ProfilePicture = a.User?.ProfilePicture,
                    Connections = a.User?.Connections ?? new List<string>()
                },
                RecommendationScore = CalculateArticleScore(a)
            }).ToList();

            return recommendations;
        }

        private async Task<List<RecommendationDto>> GetCollaborativeHandicraftRecommendationsAsync(int userId, int count)
        {
            var similarUsers = await FindSimilarUsersAsync(userId);

            var rawHandicrafts = await _context.Set<Favorite>()
                .Include(f => f.HandiCraft).ThenInclude(h => h.Category)
                .Include(f => f.HandiCraft).ThenInclude(h => h.User)
                .Where(f => similarUsers.Contains(f.UserId) && f.UserId != userId)
                .Select(f => f.HandiCraft)
                .Where(h => !_context.Set<Favorite>().Any(uf => uf.UserId == userId && uf.HandiCraftId == h.Id))
                .GroupBy(h => h.Id)
                .OrderByDescending(g => g.Count())
                .Take(count)
                .Select(g => g.First())
                .ToListAsync();

            var recommendations = rawHandicrafts.Select(h => new RecommendationDto
            {
                Type = "Handicraft",
                ItemId = h.Id,
                Title = h.Title,
                Images = h.ImageOrVideo ?? new List<string>(),
                Content = h.Description.Length > 200 ? h.Description.Substring(0, 200) + "..." : h.Description,
                CategoryName = h.Category?.Name,
                Price = h.Price,
                Creator = new CreatorDto
                {
                    UserId = h.User?.Id ?? 0,
                    FullName = $"{h.User?.FirstName} {h.User?.LastName}",
                    Email = h.User?.Email,
                    ProfilePicture = h.User?.ProfilePicture,
                    Connections = h.User?.Connections ?? new List<string>()
                },
                RecommendationScore = CalculateHandicraftScore(h)
            }).ToList();

            return recommendations;
        }

        private async Task<List<int>> FindSimilarUsersAsync(int userId)
        {
            var userEngagement = await GetUserEngagementDataAsync(userId);

            var engagedItems = userEngagement.LikedArticleIds
                .Concat(userEngagement.CommentedArticleIds)
                .Concat(userEngagement.FavoriteHandicraftIds)
                .Distinct()
                .ToList();

            var similarUsers = await _context.Set<Like>()
                .Where(l => engagedItems.Contains(l.CulturalArticleId) && l.UserId != userId)
                .Select(l => l.UserId)
                .Union(
                    _context.Set<Comment>()
                        .Where(c => engagedItems.Contains(c.CulturalArticleId) && c.UserId != userId)
                        .Select(c => c.UserId)
                )
                .Union(
                    _context.Set<Favorite>()
                        .Where(f => engagedItems.Contains(f.HandiCraftId) && f.UserId != userId)
                        .Select(f => f.UserId)
                )
                .GroupBy(u => u)
                .Where(g => g.Count() >= 2)
                .OrderByDescending(g => g.Count())
                .Take(10)
                .Select(g => g.Key)
                .ToListAsync();

            return similarUsers;
        }

        public async Task<List<RecommendationDto>> GetSimilarArticlesAsync(int articleId, int userId, int count = 5)
        {
            var targetArticle = await _context.Set<CulturalArticle>()
                .Include(a => a.Category)
                .FirstOrDefaultAsync(a => a.Id == articleId);

            if (targetArticle == null) return new List<RecommendationDto>();

            var similarArticles = await _context.Set<CulturalArticle>()
                .Include(a => a.Category)
                .Include(a => a.User)
                .Where(a => a.Id != articleId &&
                           a.CategoryId == targetArticle.CategoryId &&
                           !_context.Set<Like>().Any(l => l.UserId == userId && l.CulturalArticleId == a.Id))
                .OrderByDescending(a => a.Likes.Count())
                .Take(count)
                .Select(a => new RecommendationDto
                {
                    Type = "Article",
                    ItemId = a.Id,
                    Title = a.Title,
                    Images = a.ImageURL ?? new List<string>(),
                    Content = a.Content.Length > 200 ? a.Content.Substring(0, 200) + "..." : a.Content,
                    CategoryName = a.Category.Name,
                    Creator = new CreatorDto
                    {
                        UserId = a.User.Id,
                        FullName = $"{a.User.FirstName} {a.User.LastName}",
                        Email = a.User.Email,
                        ProfilePicture = a.User.ProfilePicture,
                        Connections = a.User.Connections ?? new List<string>()
                    },
                    RecommendationScore = CalculateArticleScore(a)
                })
                .ToListAsync();

            return similarArticles;
        }

        public async Task<List<RecommendationDto>> GetSimilarHandicraftsAsync(int handicraftId, int userId, int count = 5)
        {
            var targetHandicraft = await _context.Set<HandiCraft>()
                .Include(h => h.Category)
                .FirstOrDefaultAsync(h => h.Id == handicraftId);

            if (targetHandicraft == null) return new List<RecommendationDto>();

            var similarHandicrafts = await _context.Set<HandiCraft>()
                .Include(h => h.Category)
                .Include(h => h.User)
                .Where(h => h.Id != handicraftId &&
                           h.CategoryId == targetHandicraft.CategoryId &&
                           !_context.Set<Favorite>().Any(f => f.UserId == userId && f.HandiCraftId == h.Id))
                .OrderByDescending(h => h.Favorite.Count())
                .Take(count)
                .Select(h => new RecommendationDto
                {
                    Type = "Handicraft",
                    ItemId = h.Id,
                    Title = h.Title,
                    Images = h.ImageOrVideo ?? new List<string>(),
                    Content = h.Description.Length > 200 ? h.Description.Substring(0, 200) + "..." : h.Description,
                    CategoryName = h.Category.Name,
                    Price = h.Price,
                    Creator = new CreatorDto
                    {
                        UserId = h.User.Id,
                        FullName = $"{h.User.FirstName} {h.User.LastName}",
                        Email = h.User.Email,
                        ProfilePicture = h.User.ProfilePicture,
                        Connections = h.User.Connections ?? new List<string>()
                    },
                    RecommendationScore = CalculateHandicraftScore(h)
                })
                .ToListAsync();

            return similarHandicrafts;
        }

        public async Task UpdateUserEngagementAsync(int userId, string engagementType, int itemId)
        {
            await Task.CompletedTask;
        }

        private static double CalculateArticleScore(CulturalArticle article)
        {
            var likesWeight = 0.4;
            var commentsWeight = 0.3;
            var recencyWeight = 0.3;

            var likes = article.Likes?.Count() ?? 0;
            var comments = article.Comments?.Count() ?? 0;
            var daysSinceCreated = (DateTime.UtcNow - article.DateCreated).Days;
            var recencyScore = Math.Max(0, 30 - daysSinceCreated) / 30.0;

            return (likes * likesWeight) + (comments * commentsWeight) + (recencyScore * recencyWeight);
        }

        private static double CalculateHandicraftScore(HandiCraft handicraft)
        {
            var favoritesWeight = 0.5;
            var recencyWeight = 0.3;
            var priceWeight = 0.2;

            var favorites = handicraft.Favorite?.Count() ?? 0;
            var daysSinceCreated = (DateTime.UtcNow - handicraft.DateAdded).Days;
            var recencyScore = Math.Max(0, 30 - daysSinceCreated) / 30.0;
            var priceScore = Math.Max(0, 1000 - handicraft.Price) / 1000.0;

            return (favorites * favoritesWeight) + (recencyScore * recencyWeight) + (priceScore * priceWeight);
        }

        private static List<RecommendationDto> CombineAndRankRecommendations(
            List<RecommendationDto> list1,
            List<RecommendationDto> list2)
        {
            var combined = list1.Concat(list2)
                .GroupBy(r => new { r.Type, r.ItemId })
                .Select(g => g.OrderByDescending(r => r.RecommendationScore).First())
                .OrderByDescending(r => r.RecommendationScore)
                .ToList();

            return combined;
        }
    }
}