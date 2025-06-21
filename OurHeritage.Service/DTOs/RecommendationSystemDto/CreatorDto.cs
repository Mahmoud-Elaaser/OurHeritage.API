namespace OurHeritage.Service.DTOs.RecommendationSystemDto
{
    public class CreatorDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string ProfilePicture { get; set; }
        public List<string> Connections { get; set; }
    }
}
