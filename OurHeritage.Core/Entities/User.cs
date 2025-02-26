using Microsoft.AspNetCore.Identity;
using OurHeritage.Core.Enums;

namespace OurHeritage.Core.Entities
{
    public class User : IdentityUser<int>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? ProfilePicture { get; set; }
        public string? CoverProfilePicture { get; set; }
        public string Gender { get; set; }
        public DateTime DateJoined { get; set; } = DateTime.UtcNow;
        public string Email { get; set; }
        public string? Phone { get; set; }
        public List<string>? Skills { get; set; }
        public List<string>? Connections { get; set; }


        public ICollection<HandiCraft>? HandiCrafts { get; set; }
        public ICollection<CulturalArticle>? culturalArticles { get; set; }
        public ICollection<Order>? Orders { get; set; }
        public ICollection<Comment>? Comments { get; set; }
        public ICollection<Like>? Likes { get; set; }
        public ICollection<User> Followings { get; set; }
        public ICollection<User> Followers { get; set; }
    }
}
