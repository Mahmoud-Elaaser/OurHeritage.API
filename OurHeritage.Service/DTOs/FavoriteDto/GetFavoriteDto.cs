namespace OurHeritage.Service.DTOs.FavoriteDto
{
    public class GetFavoriteDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string NameOfUser { get; set; }
        public string UserProfilePicture { get; set; }
        public int HandiCraftId { get; set; }
    }
}
