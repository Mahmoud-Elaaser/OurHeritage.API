namespace OurHeritage.Service.DTOs.LikeDto
{
    public class GetLikeDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int CulturalArticleId { get; set; }
        public DateTime LikedAt { get; set; }
        public string NameOfUser { get; set; }
        public string UserProfilePicture { get; set; }
    }
}