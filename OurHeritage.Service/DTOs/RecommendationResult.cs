using OurHeritage.Service.Interfaces;

namespace OurHeritage.Service.DTOs
{
    public class RecommendationResult
    {
        public IRecommendableItem Item { get; set; }
        public double Score { get; set; }
        public string ReasonForRecommendation { get; set; }
        public List<string> RecommendationFactors { get; set; } = new List<string>();
    }
}
