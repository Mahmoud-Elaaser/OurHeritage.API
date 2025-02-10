using Microsoft.AspNetCore.Identity;
using OurHeritage.Core.Enums;

namespace OurHeritage.Core.Entities
{
    public class User : IdentityUser<int>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string? ProfilePicture { get; set; }
        public Gender Gender { get; set; }
        public DateTime DateJoined { get; set; } = DateTime.UtcNow;
    }
}
