using OurHeritage.Service.DTOs.UserDto;

namespace OurHeritage.Service.DTOs.RepostDto
{
    public class GetRepostDto
    {
        public int Id { get; set; }
        public string? Content { get; set; }
        public int UserId { get; set; }
        public int CulturalArticleId { get; set; }
        public GetUserDto User { get; set; }
    }
}
