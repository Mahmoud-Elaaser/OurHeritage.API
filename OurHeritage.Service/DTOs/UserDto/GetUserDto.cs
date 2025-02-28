using OurHeritage.Core.Enums;
namespace OurHeritage.Service.DTOs.UserDto
{
    public class GetUserDto
    {
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        public string? FullName { get; set; }

        public string? ProfilePicture { get; set; }
        public Gender Gender { get; set; }

        public string? CoverProfilePicture { get; set; }
        public DateTime DateJoined { get; set; }
        public string? Phone { get; set; }
        public List<string>? Skills { get; set; }
        public List<string>? Connections { get; set; }
    }
}