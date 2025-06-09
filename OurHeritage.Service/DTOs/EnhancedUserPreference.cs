namespace OurHeritage.Service.DTOs
{
    public class EnhancedUserPreference
    {
        public int UserId { get; set; }
        public Dictionary<int, double> CategoryPreferences { get; set; } = new();
        public Dictionary<string, double> ContentTypePreferences { get; set; } = new();
        public List<int> FavoriteCulturals { get; set; } = new();
        public List<int> FavoriteHandicrafts { get; set; } = new();
        public List<int> OrderedHandicrafts { get; set; } = new();
        public Dictionary<string, double> PriceRangePreferences { get; set; } = new();
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}
