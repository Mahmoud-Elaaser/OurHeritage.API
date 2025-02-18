namespace OurHeritage.Service.DTOs.UserDto
{
    public class GetUserDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string? ProfilePicture { get; set; }
        public string Gender { get; set; }
        public DateTime DateJoined { get; set; } = DateTime.UtcNow;

    }
}
