using Microsoft.Extensions.Logging;
using OurHeritage.Core.Entities;
using OurHeritage.Core.Enums;
using OurHeritage.Repo.Repositories.Interfaces;
using OurHeritage.Service.DTOs;
using OurHeritage.Service.Interfaces;


namespace OurHeritage.Service.Implementations
{
    public class RecommendationService : IRecommendationService
    {
        private readonly IGenericRepository<CulturalArticle> _culturalRepository;
        private readonly IGenericRepository<HandiCraft> _handicraftRepository;
        private readonly IGenericRepository<Category> _categoryRepository;
        private readonly IGenericRepository<Favorite> _favoriteRepository;
        private readonly IGenericRepository<Order> _orderRepository;
        private readonly ILogger<RecommendationService> _logger;
        private readonly Dictionary<int, EnhancedUserPreference> _userPreferencesCache = new();

        public RecommendationService(
            IGenericRepository<CulturalArticle> culturalRepository,
            IGenericRepository<HandiCraft> handicraftRepository,
            IGenericRepository<Category> categoryRepository,
            IGenericRepository<Favorite> favoriteRepository,
            IGenericRepository<Order> orderRepository,
            ILogger<RecommendationService> logger)
        {
            _culturalRepository = culturalRepository;
            _handicraftRepository = handicraftRepository;
            _categoryRepository = categoryRepository;
            _favoriteRepository = favoriteRepository;
            _orderRepository = orderRepository;
            _logger = logger;
        }

        public async Task<List<RecommendationResult>> GetRecommendationsAsync(int userId, RecommendationType type = RecommendationType.Mixed, int count = 10)
        {
            try
            {
                await UpdateUserPreferencesAsync(userId);

                return type switch
                {
                    RecommendationType.Cultural => await GetCulturalRecommendationsAsync(userId, count),
                    RecommendationType.Handicraft => await GetHandicraftRecommendationsAsync(userId, count),
                    RecommendationType.Mixed => await GetMixedRecommendationsAsync(userId, count),
                    _ => await GetMixedRecommendationsAsync(userId, count)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recommendations for user {UserId}", userId);
                return new List<RecommendationResult>();
            }
        }

        public async Task<List<RecommendationResult>> GetCulturalRecommendationsAsync(int userId, int count = 10)
        {
            var userPrefs = await GetUserPreferencesAsync(userId);
            var culturals = await _culturalRepository.ListAllAsync();

            var recommendations = new List<RecommendationResult>();

            foreach (var cultural in culturals)
            {
                var item = new CulturalRecommendationItem(cultural);
                var score = CalculateCulturalScore(item, userPrefs);
                var factors = GetCulturalRecommendationFactors(item, userPrefs);

                recommendations.Add(new RecommendationResult
                {
                    Item = item,
                    Score = score,
                    ReasonForRecommendation = GenerateReasonForCultural(factors),
                    RecommendationFactors = factors
                });
            }

            return recommendations.OrderByDescending(r => r.Score).Take(count).ToList();
        }

        public async Task<List<RecommendationResult>> GetHandicraftRecommendationsAsync(int userId, int count = 10)
        {
            var userPrefs = await GetUserPreferencesAsync(userId);
            var handicrafts = await _handicraftRepository.ListAllAsync();
            var userFavorites = userPrefs.FavoriteHandicrafts;
            var userOrdered = userPrefs.OrderedHandicrafts;

            var recommendations = new List<RecommendationResult>();

            foreach (var handicraft in handicrafts.Where(h => !userFavorites.Contains(h.Id) && !userOrdered.Contains(h.Id)))
            {
                var item = new HandicraftRecommendationItem(handicraft);
                var score = CalculateHandicraftScore(item, userPrefs);
                var factors = GetHandicraftRecommendationFactors(item, userPrefs);

                recommendations.Add(new RecommendationResult
                {
                    Item = item,
                    Score = score,
                    ReasonForRecommendation = GenerateReasonForHandicraft(factors),
                    RecommendationFactors = factors
                });
            }

            return recommendations.OrderByDescending(r => r.Score).Take(count).ToList();
        }

        private async Task<List<RecommendationResult>> GetMixedRecommendationsAsync(int userId, int count = 10)
        {
            var culturalCount = count / 2;
            var handicraftCount = count - culturalCount;

            var culturalRecommendations = await GetCulturalRecommendationsAsync(userId, culturalCount);
            var handicraftRecommendations = await GetHandicraftRecommendationsAsync(userId, handicraftCount);

            var mixedRecommendations = new List<RecommendationResult>();
            mixedRecommendations.AddRange(culturalRecommendations);
            mixedRecommendations.AddRange(handicraftRecommendations);

            return mixedRecommendations.OrderByDescending(r => r.Score).Take(count).ToList();
        }

        public async Task<List<RecommendationResult>> GetSimilarHandicraftsAsync(int handicraftId, int count = 5)
        {
            var targetHandicraft = await _handicraftRepository.GetByIdAsync(handicraftId);
            if (targetHandicraft == null) return new List<RecommendationResult>();

            var allHandicrafts = await _handicraftRepository.ListAllAsync();
            var recommendations = new List<RecommendationResult>();

            foreach (var handicraft in allHandicrafts.Where(h => h.Id != handicraftId))
            {
                var similarity = CalculateHandicraftSimilarity(targetHandicraft, handicraft);
                if (similarity > 0.3) // Threshold for similarity
                {
                    var item = new HandicraftRecommendationItem(handicraft);
                    recommendations.Add(new RecommendationResult
                    {
                        Item = item,
                        Score = similarity,
                        ReasonForRecommendation = $"Similar to {targetHandicraft.Title}",
                        RecommendationFactors = new List<string> { "Similar category", "Similar price range" }
                    });
                }
            }

            return recommendations.OrderByDescending(r => r.Score).Take(count).ToList();
        }

        public async Task<List<RecommendationResult>> GetSimilarCulturalsAsync(int culturalId, int count = 5)
        {
            var targetCultural = await _culturalRepository.GetByIdAsync(culturalId);
            if (targetCultural == null) return new List<RecommendationResult>();

            var allCulturals = await _culturalRepository.ListAllAsync();
            var recommendations = new List<RecommendationResult>();

            foreach (var cultural in allCulturals.Where(c => c.Id != culturalId))
            {
                var similarity = CalculateCulturalSimilarity(targetCultural, cultural);
                if (similarity > 0.3)
                {
                    var item = new CulturalRecommendationItem(cultural);
                    recommendations.Add(new RecommendationResult
                    {
                        Item = item,
                        Score = similarity,
                        ReasonForRecommendation = $"Similar to {targetCultural.Title}",
                        RecommendationFactors = new List<string> { "Similar category", "Similar content" }
                    });
                }
            }

            return recommendations.OrderByDescending(r => r.Score).Take(count).ToList();
        }

        public async Task<List<RecommendationResult>> GetTrendingHandicraftsAsync(int count = 10)
        {
            var handicrafts = await _handicraftRepository.ListAllAsync();
            var recommendations = new List<RecommendationResult>();

            foreach (var handicraft in handicrafts)
            {
                var trendingScore = CalculateTrendingScore(handicraft);
                var item = new HandicraftRecommendationItem(handicraft);

                recommendations.Add(new RecommendationResult
                {
                    Item = item,
                    Score = trendingScore,
                    ReasonForRecommendation = "Trending now",
                    RecommendationFactors = new List<string> { "Popular", "High engagement" }
                });
            }

            return recommendations.OrderByDescending(r => r.Score).Take(count).ToList();
        }

        public async Task<List<RecommendationResult>> GetRecentHandicraftsAsync(int count = 10)
        {
            var handicrafts = await _handicraftRepository.ListAllAsync();
            var recentHandicrafts = handicrafts
                .OrderByDescending(h => h.DateAdded)
                .Take(count)
                .ToList();

            var recommendations = new List<RecommendationResult>();
            foreach (var handicraft in recentHandicrafts)
            {
                var item = new HandicraftRecommendationItem(handicraft);
                recommendations.Add(new RecommendationResult
                {
                    Item = item,
                    Score = 1.0,
                    ReasonForRecommendation = "Recently added",
                    RecommendationFactors = new List<string> { "New arrival" }
                });
            }

            return recommendations;
        }

        public async Task<List<RecommendationResult>> GetHandicraftsByPriceRangeAsync(int userId, double minPrice, double maxPrice, int count = 10)
        {
            var userPrefs = await GetUserPreferencesAsync(userId);
            var handicrafts = await _handicraftRepository.ListAllAsync();

            var filteredHandicrafts = handicrafts.Where(h => h.Price >= minPrice && h.Price <= maxPrice);
            var recommendations = new List<RecommendationResult>();

            foreach (var handicraft in filteredHandicrafts)
            {
                var item = new HandicraftRecommendationItem(handicraft);
                var score = CalculateHandicraftScore(item, userPrefs);

                recommendations.Add(new RecommendationResult
                {
                    Item = item,
                    Score = score,
                    ReasonForRecommendation = $"Within your price range (${minPrice:F2} - ${maxPrice:F2})",
                    RecommendationFactors = new List<string> { "Price match", "Category preference" }
                });
            }

            return recommendations.OrderByDescending(r => r.Score).Take(count).ToList();
        }

        public async Task UpdateUserPreferencesAsync(int userId)
        {
            try
            {
                var userPref = new EnhancedUserPreference { UserId = userId };

                // Get user's favorite handicrafts
                var favorites = await _favoriteRepository.ListAllAsync();
                var userFavoriteHandicrafts = favorites.Where(f => f.UserId == userId).ToList();

                foreach (var favorite in userFavoriteHandicrafts)
                {
                    userPref.FavoriteHandicrafts.Add(favorite.HandiCraftId);
                    var handicraft = await _handicraftRepository.GetByIdAsync(favorite.HandiCraftId);
                    if (handicraft != null)
                    {
                        UpdateCategoryPreference(userPref.CategoryPreferences, handicraft.CategoryId, 1.0);
                        UpdatePriceRangePreference(userPref.PriceRangePreferences, handicraft.Price);
                    }
                }

                // Get user's orders for handicrafts
                var orders = await _orderRepository.ListAllAsync();
                var userOrders = orders.Where(o => o.UserId == userId && o.IsPaid).ToList();

                foreach (var order in userOrders)
                {
                    // Process all handicrafts in each order
                    foreach (var handicraft in order.HandiCrafts)
                    {
                        userPref.OrderedHandicrafts.Add(handicraft.Id);
                        UpdateCategoryPreference(userPref.CategoryPreferences, handicraft.CategoryId, 1.5); // Higher weight for purchases
                        UpdatePriceRangePreference(userPref.PriceRangePreferences, handicraft.Price);
                    }
                }

                _userPreferencesCache[userId] = userPref;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user preferences for user {UserId}", userId);
            }
        }

        private async Task<EnhancedUserPreference> GetUserPreferencesAsync(int userId)
        {
            if (!_userPreferencesCache.ContainsKey(userId) ||
                _userPreferencesCache[userId].LastUpdated < DateTime.UtcNow.AddHours(-1))
            {
                await UpdateUserPreferencesAsync(userId);
            }

            return _userPreferencesCache.GetValueOrDefault(userId, new EnhancedUserPreference { UserId = userId });
        }

        private double CalculateCulturalScore(CulturalRecommendationItem item, EnhancedUserPreference userPrefs)
        {
            double score = 0.0;

            // Category preference
            if (userPrefs.CategoryPreferences.ContainsKey(item.CategoryId))
            {
                score += userPrefs.CategoryPreferences[item.CategoryId] * 0.6;
            }

            // Recency boost
            var daysSinceAdded = (DateTime.UtcNow - item.DateAdded).TotalDays;
            if (daysSinceAdded < 30)
            {
                score += (30 - daysSinceAdded) / 30 * 0.2;
            }

            // Content type preference
            score += 0.2; // Base score for cultural content

            return Math.Min(score, 1.0);
        }

        private double CalculateHandicraftScore(HandicraftRecommendationItem item, EnhancedUserPreference userPrefs)
        {
            double score = 0.0;

            // Category preference
            if (userPrefs.CategoryPreferences.ContainsKey(item.CategoryId))
            {
                score += userPrefs.CategoryPreferences[item.CategoryId] * 0.4;
            }

            // Price range preference
            var priceRange = GetPriceRange(item.Price);
            if (userPrefs.PriceRangePreferences.ContainsKey(priceRange))
            {
                score += userPrefs.PriceRangePreferences[priceRange] * 0.3;
            }

            // Recency boost
            var daysSinceAdded = (DateTime.UtcNow - item.DateAdded).TotalDays;
            if (daysSinceAdded < 30)
            {
                score += (30 - daysSinceAdded) / 30 * 0.2;
            }

            // Base score for handicrafts
            score += 0.1;

            return Math.Min(score, 1.0);
        }

        private double CalculateHandicraftSimilarity(HandiCraft target, HandiCraft candidate)
        {
            double similarity = 0.0;

            // Category similarity
            if (target.CategoryId == candidate.CategoryId)
            {
                similarity += 0.5;
            }

            // Price similarity
            var priceDifference = Math.Abs(target.Price - candidate.Price);
            var maxPrice = Math.Max(target.Price, candidate.Price);
            if (maxPrice > 0)
            {
                var priceSimiliarity = 1.0 - (priceDifference / maxPrice);
                similarity += priceSimiliarity * 0.3;
            }

            // Description similarity (basic text comparison)
            var descriptionSimilarity = CalculateTextSimilarity(target.Description, candidate.Description);
            similarity += descriptionSimilarity * 0.2;

            return similarity;
        }

        private double CalculateCulturalSimilarity(CulturalArticle target, CulturalArticle candidate)
        {
            double similarity = 0.0;

            // Category similarity
            if (target.CategoryId == candidate.CategoryId)
            {
                similarity += 0.6;
            }

            // Description similarity
            var descriptionSimilarity = CalculateTextSimilarity(target.Title, candidate.Title);
            similarity += descriptionSimilarity * 0.4;

            return similarity;
        }

        private double CalculateTrendingScore(HandiCraft handicraft)
        {
            // This is a simplified trending calculation
            // In a real implementation, you might consider:
            // - Number of recent favorites
            // - Number of recent orders
            // - View count, etc.

            var daysSinceAdded = (DateTime.UtcNow - handicraft.DateAdded).TotalDays;
            var recencyScore = Math.Max(0, (30 - daysSinceAdded) / 30);

            // Add some randomness to simulate engagement metrics
            var random = new Random(handicraft.Id);
            var engagementScore = random.NextDouble();

            return (recencyScore * 0.6) + (engagementScore * 0.4);
        }

        private double CalculateTextSimilarity(string text1, string text2)
        {
            if (string.IsNullOrEmpty(text1) || string.IsNullOrEmpty(text2))
                return 0.0;

            var words1 = text1.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var words2 = text2.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);

            var intersection = words1.Intersect(words2).Count();
            var union = words1.Union(words2).Count();

            return union > 0 ? (double)intersection / union : 0.0;
        }

        private void UpdateCategoryPreference(Dictionary<int, double> categoryPrefs, int categoryId, double weight)
        {
            if (categoryPrefs.ContainsKey(categoryId))
            {
                categoryPrefs[categoryId] += weight;
            }
            else
            {
                categoryPrefs[categoryId] = weight;
            }
        }

        private void UpdatePriceRangePreference(Dictionary<string, double> priceRangePrefs, double price)
        {
            var priceRange = GetPriceRange(price);
            if (priceRangePrefs.ContainsKey(priceRange))
            {
                priceRangePrefs[priceRange] += 1.0;
            }
            else
            {
                priceRangePrefs[priceRange] = 1.0;
            }
        }

        private string GetPriceRange(double price)
        {
            return price switch
            {
                < 25 => "budget",
                < 100 => "affordable",
                < 500 => "moderate",
                < 1000 => "premium",
                _ => "luxury"
            };
        }

        private List<string> GetCulturalRecommendationFactors(CulturalRecommendationItem item, EnhancedUserPreference userPrefs)
        {
            var factors = new List<string>();

            if (userPrefs.CategoryPreferences.ContainsKey(item.CategoryId))
            {
                factors.Add("Matches your category interests");
            }

            if ((DateTime.UtcNow - item.DateAdded).TotalDays < 7)
            {
                factors.Add("Recently added");
            }

            return factors;
        }

        private List<string> GetHandicraftRecommendationFactors(HandicraftRecommendationItem item, EnhancedUserPreference userPrefs)
        {
            var factors = new List<string>();

            if (userPrefs.CategoryPreferences.ContainsKey(item.CategoryId))
            {
                factors.Add("Matches your category interests");
            }

            var priceRange = GetPriceRange(item.Price);
            if (userPrefs.PriceRangePreferences.ContainsKey(priceRange))
            {
                factors.Add($"Within your preferred {priceRange} price range");
            }

            if ((DateTime.UtcNow - item.DateAdded).TotalDays < 7)
            {
                factors.Add("Recently added");
            }

            return factors;
        }

        private string GenerateReasonForCultural(List<string> factors)
        {
            if (factors.Any())
            {
                return $"Recommended because: {string.Join(", ", factors)}";
            }
            return "You might find this cultural item interesting";
        }

        private string GenerateReasonForHandicraft(List<string> factors)
        {
            if (factors.Any())
            {
                return $"Recommended because: {string.Join(", ", factors)}";
            }
            return "You might like this handicraft";
        }
    }

    // Extension methods for easier usage
    public static class RecommendationExtensions
    {
        public static List<CulturalArticle> ToCulturalList(this List<RecommendationResult> recommendations)
        {
            return recommendations
                .Where(r => r.Item is CulturalRecommendationItem)
                .Select(r => ((CulturalRecommendationItem)r.Item).Cultural)
                .ToList();
        }

        public static List<HandiCraft> ToHandicraftList(this List<RecommendationResult> recommendations)
        {
            return recommendations
                .Where(r => r.Item is HandicraftRecommendationItem)
                .Select(r => ((HandicraftRecommendationItem)r.Item).HandiCraft)
                .ToList();
        }

        public static List<RecommendationResult> GetHandicraftRecommendations(this List<RecommendationResult> recommendations)
        {
            return recommendations.Where(r => r.Item is HandicraftRecommendationItem).ToList();
        }

        public static List<RecommendationResult> GetCulturalRecommendations(this List<RecommendationResult> recommendations)
        {
            return recommendations.Where(r => r.Item is CulturalRecommendationItem).ToList();
        }
    }

}
