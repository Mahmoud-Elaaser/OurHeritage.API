namespace OurHeritage.Service.Interfaces
{
    public interface IRecommendableItem
    {
        int Id { get; }
        string Title { get; }
        string Description { get; }
        int CategoryId { get; }
        int UserId { get; }
        DateTime DateAdded { get; }
        string ItemType { get; }
    }
}
