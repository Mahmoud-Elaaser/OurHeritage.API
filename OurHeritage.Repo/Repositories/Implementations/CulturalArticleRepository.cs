using Microsoft.EntityFrameworkCore;
using OurHeritage.Core.Context;
using OurHeritage.Repo.Repositories.Interfaces;

namespace OurHeritage.Repo.Repositories.Implementations
{
    public class CulturalArticleRepository : ICulturalArticleRepository
    {
        private readonly ApplicationDbContext _context;

        public CulturalArticleRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(int likeCount, int commentCount)> GetArticleLikeAndCommentCountAsync(int articleId)
        {
            var article = await _context.CulturalArticles
                .Where(a => a.Id == articleId)
                .Select(a => new
                {
                    LikeCount = a.Likes.Count,
                    CommentCount = a.Comments.Count
                })
                .FirstOrDefaultAsync();

            return article == null ? (0, 0) : (article.LikeCount, article.CommentCount);
        }
    }

}
