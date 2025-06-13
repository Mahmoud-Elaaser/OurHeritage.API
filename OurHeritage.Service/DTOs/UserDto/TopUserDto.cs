namespace OurHeritage.Service.DTOs.UserDto
{
    public class TopUserDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string ProfilePicture { get; set; }
        public int ArticleCount { get; set; }
        public int HandiCraftCount { get; set; }
        public int TotalLikes { get; set; }
        public int FollowersCount { get; set; }
        public int ActivityScore { get; set; }
    }
}
