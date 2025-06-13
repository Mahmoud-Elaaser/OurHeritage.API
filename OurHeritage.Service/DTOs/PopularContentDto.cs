namespace OurHeritage.Service.DTOs
{
    public class PopularContentDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string CreatorName { get; set; }
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }
        public int FavoriteCount { get; set; }
        public DateTime DateCreated { get; set; }
        public string ContentType { get; set; } // "Article" or "HandiCraft"
    }
}
