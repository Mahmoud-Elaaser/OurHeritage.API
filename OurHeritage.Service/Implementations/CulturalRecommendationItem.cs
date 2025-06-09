using OurHeritage.Core.Entities;
using OurHeritage.Service.Interfaces;

namespace OurHeritage.Service.Implementations
{
    public class CulturalRecommendationItem : IRecommendableItem
    {
        private readonly CulturalArticle _cultural;

        public CulturalRecommendationItem(CulturalArticle cultural)
        {
            _cultural = cultural;
        }

        public int Id => _cultural.Id;
        public string Title => _cultural.Title;
        public string Description => _cultural.Content;
        public int CategoryId => _cultural.CategoryId;
        public int UserId => _cultural.UserId;
        public DateTime DateAdded => _cultural.DateCreated;
        public string ItemType => "CulturalArticle";

        public CulturalArticle Cultural => _cultural;
    }
}
