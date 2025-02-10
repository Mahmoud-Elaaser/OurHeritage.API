using System.ComponentModel.DataAnnotations;

namespace OurHeritage.Service.DTOs.AuthDto
{
    public class ResetPasswordDto
    {
        [Required]
        public string OtpCode { get; set; } // send forgot then receive otp via email
        [Required]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Compare(nameof(NewPassword))]
        public string ConfirmPassword { get; set; }
    }
}
