namespace OurHeritage.Service.DTOs.CulturalArticleDto
{
    public class GetCulturalArticleDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public List<string> ImageURL { get; set; }
        public int CategoryId { get; set; }
        public int UserId { get; set; }
        public string NameOfUser { get; set; }
        public string UserProfilePicture { get; set; }
        public string NameOfCategory { get; set; }
        public string DateCreated { get; set; } // Formatted Date
        public string TimeAgo { get; set; } // Time ago format
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }
        public bool IsFollowing { get; set; }
    }
}
