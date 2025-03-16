namespace OurHeritage.Repo.Repositories.Interfaces
{
    public interface ICulturalArticleRepository
    {
        Task<(int likeCount, int commentCount)> GetArticleLikeAndCommentCountAsync(int articleId);
    }

}
