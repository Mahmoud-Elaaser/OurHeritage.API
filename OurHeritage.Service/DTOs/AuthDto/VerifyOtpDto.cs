using System.ComponentModel.DataAnnotations;

namespace OurHeritage.Service.DTOs.AuthDto
{
    public class VerifyOtpDto
    {
        [Required]
        public string OtpCode { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
