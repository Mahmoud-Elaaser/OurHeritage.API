using OurHeritage.Core.Entities;
using OurHeritage.Service.Interfaces;

namespace OurHeritage.Service.Implementations
{
    public class HandicraftRecommendationItem : IRecommendableItem
    {
        private readonly HandiCraft _handicraft;

        public HandicraftRecommendationItem(HandiCraft handicraft)
        {
            _handicraft = handicraft;
        }

        public int Id => _handicraft.Id;
        public string Title => _handicraft.Title;
        public string Description => _handicraft.Description;
        public int CategoryId => _handicraft.CategoryId;
        public int UserId => _handicraft.UserId;
        public DateTime DateAdded => _handicraft.DateAdded;
        public string ItemType => "HandiCraft";

        public HandiCraft HandiCraft => _handicraft;
        public double Price => _handicraft.Price;
    }

}
