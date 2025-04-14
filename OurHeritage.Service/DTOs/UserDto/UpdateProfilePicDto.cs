using Microsoft.AspNetCore.Http;

namespace OurHeritage.Service.DTOs.UserDto
{
    public class UpdateProfilePicDto
    {
        public IFormFile? ImageProfile { get; set; }
    }
}
