using Microsoft.AspNetCore.Http;

namespace OurHeritage.Service.DTOs.UserDto
{
    public class UpdateCoverPhotoDto
    {
        public IFormFile? ImageCover { get; set; }
    }
}
