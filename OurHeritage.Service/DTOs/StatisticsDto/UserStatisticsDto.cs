namespace OurHeritage.Service.DTOs.StatisticsDto
{
    public class UserStatisticsDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string ProfilePicture { get; set; }
        public DateTime DateJoined { get; set; }
        public int ArticleCount { get; set; }
        public int HandiCraftCount { get; set; }
        public int FollowersCount { get; set; }
        public int FollowingCount { get; set; }
        public bool IsActive { get; set; }
    }
}
