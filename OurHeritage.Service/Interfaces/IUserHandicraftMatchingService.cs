namespace OurHeritage.Service.Interfaces
{
    public interface IUserHandicraftMatchingService
    {
        //Task<List<UserMatchResult>> FindPerfectCraftersAsync(string handicraftDescription, int handicraftId = 0, int maxResults = 5);
        Task<List<UserMatchResult>> FindTopUsersByHandiCraftCategoryAsync(string categoryName);
    }
}
