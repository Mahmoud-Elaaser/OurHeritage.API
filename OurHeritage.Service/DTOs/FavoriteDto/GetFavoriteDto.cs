namespace OurHeritage.Service.DTOs.FavoriteDto
{
    public class GetFavoriteDto
    {
        public int Id { get; set; }
        public int HandiCraftId { get; set; }
        public int UserId { get; set; }
        public string CreatorName { get; set; }
        public string HandiCraftTitle { get; set; }
        public string CreatorProfilePicture { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
