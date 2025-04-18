using Microsoft.EntityFrameworkCore;
using OurHeritage.Core.Context;
using OurHeritage.Core.Entities;
using OurHeritage.Service.Interfaces;

namespace OurHeritage.Service.Implementations
{
    public class UserHandicraftMatchingService : IUserHandicraftMatchingService
    {
        private readonly ApplicationDbContext _dbContext;

        public UserHandicraftMatchingService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<UserMatchResult>> FindTopUsersByHandiCraftCategoryAsync(string categoryName)
        {
            var category = await _dbContext.Categories
                .FirstOrDefaultAsync(c => c.Name.ToLower() == categoryName.ToLower());

            if (category == null)
                return new List<UserMatchResult>();

            var users = await _dbContext.Users
                .Include(u => u.HandiCrafts)
                .Where(u => u.Skills != null && u.Skills.Contains(category.Name))
                .ToListAsync();

            var userMatches = users
                .Select(user =>
                {
                    var matchingSkills = user.Skills
                        ?.Where(skill => skill.Equals(category.Name, StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    var relevantCount = user.HandiCrafts?
                        .Count(h => h.CategoryId == category.Id) ?? 0;

                    return new UserMatchResult
                    {
                        UserId = user.Id,
                        FullName = $"{user.FirstName} {user.LastName}",
                        ProfilePicture = user.ProfilePicture,
                        //MatchingSkills = matchingSkills,
                        RelevantHandiCraftsCount = relevantCount,
                        Phone = user.Phone,
                        Connections = user.Connections,
                        RelevantSkills = matchingSkills
                    };
                })
                .OrderByDescending(u => u.RelevantHandiCraftsCount)
                .ToList();

            return userMatches;
        }





        #region helper methods
        /// Extracts relevant keywords from the handicraft description
        private List<string> ExtractKeywords(string description)
        {
            if (string.IsNullOrEmpty(description))
                return new List<string>();


            var commonWords = new[] {
                "and", "or", "the", "a", "an", "in", "on", "at", "to",
                "for", "with", "by", "of", "as", "this", "that", "is", "are"
            };

            return description
                .ToLowerInvariant()
                .Split(new[] { ' ', ',', '.', ';', ':', '-', '!', '?', '(', ')' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(word => word.Length > 2 && !commonWords.Contains(word))
                .Distinct()
                .ToList();
        }

        /// Calculates how well a user matches the handicraft requirements
        private int CalculateMatchScore(User user, List<string> keywords)
        {
            int score = 0;

            // Check if user has relevant skills - Skills is now handled correctly as a List<string>
            if (user.Skills != null && user.Skills.Any())
            {
                foreach (var skill in user.Skills)
                {
                    foreach (var keyword in keywords)
                    {
                        if (skill.ToLowerInvariant().Contains(keyword))
                        {
                            score += 3; // Higher weight for direct skill matches
                        }
                    }
                }
            }

            // Check if user has created similar handicrafts before
            if (user.HandiCrafts != null && user.HandiCrafts.Any())
            {
                foreach (var craft in user.HandiCrafts)
                {
                    foreach (var keyword in keywords)
                    {
                        if (craft.Title.ToLowerInvariant().Contains(keyword))
                        {
                            score += 2;
                        }

                        if (craft.Description.ToLowerInvariant().Contains(keyword))
                        {
                            score += 1;
                        }
                    }

                    // Bonus points for having crafts in the same category
                    if (craft.Category != null && keywords.Any(k => craft.Category.Name?.ToLowerInvariant().Contains(k) == true))
                    {
                        score += 3;
                    }
                }

                // Bonus for experience (number of crafts created)
                score += Math.Min(user.HandiCrafts.Count, 5); // Cap at 5 bonus points
            }

            return score;
        }
        /// Gets skills that are relevant to the keywords
        private List<string> GetRelevantSkills(List<string> skills, List<string> keywords)
        {
            if (skills == null || !skills.Any())
                return new List<string>();

            return skills
                .Where(skill => keywords.Any(k => skill.ToLowerInvariant().Contains(k)))
                .ToList();
        }
        #endregion
    }


}

public class UserMatchResult
{
    public int UserId { get; set; }
    public string FullName { get; set; }
    public string? ProfilePicture { get; set; }
    public List<string>? RelevantSkills { get; set; }
    public int RelevantHandiCraftsCount { get; set; }
    public string Phone { get; set; }
    public List<string> Connections { get; set; }
    //public int MatchScore { get; set; }
}



/*
 *         /// Finds the most suitable users who can create a specific handicraft based on description or name
        //public async Task<List<UserMatchResult>> FindPerfectCraftersAsync(string handicraftDescription, int handicraftId = 0, int maxResults = 5)
        //{
        //    if (string.IsNullOrWhiteSpace(handicraftDescription) && handicraftId == 0)
        //        throw new ArgumentException("Either handicraft description or ID must be provided");

        //    // Extract keywords from the description
        //    var keywords = ExtractKeywords(handicraftDescription);

        //    var users = await _dbContext.Users
        //        .Include(u => u.HandiCrafts)
        //            .ThenInclude(h => h.Category)
        //        .ToListAsync();

        //    var matchResults = new List<UserMatchResult>();

        //    foreach (var user in users)
        //    {
        //        // Check if user is the creator of this specific handicraft
        //        bool isCreator = handicraftId > 0 && user.HandiCrafts.Any(h => h.Id == handicraftId);

        //        // If the user is the creator, give them a very high score
        //        int matchScore = isCreator ? 100 : CalculateMatchScore(user, keywords);

        //        if (matchScore > 0)
        //        {
        //            matchResults.Add(new UserMatchResult
        //            {
        //                UserId = user.Id,
        //                MatchScore = matchScore,
        //                RelevantSkills = GetRelevantSkills(user.Skills, keywords)
        //            });
        //        }
        //    }


        //    return matchResults
        //        .OrderByDescending(r => r.MatchScore)
        //        .Take(maxResults)
        //        .ToList();
        //}


 * */