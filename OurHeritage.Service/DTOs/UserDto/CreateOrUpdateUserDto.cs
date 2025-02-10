using OurHeritage.Core.Enums;

namespace OurHeritage.Service.DTOs.UserDto
{
    public class CreateOrUpdateUserDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? ProfilePicture { get; set; }
        public Gender Gender { get; set; }
    }
}
