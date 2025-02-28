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
    }
}
