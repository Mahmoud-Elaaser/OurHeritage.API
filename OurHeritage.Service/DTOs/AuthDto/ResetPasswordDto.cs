using System.ComponentModel.DataAnnotations;

namespace OurHeritage.Service.DTOs.AuthDto
{
    public class ResetPasswordDto
    {
        [Required]
        public string ResetToken { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required]
        [Compare(nameof(NewPassword))]
        public string ConfirmPassword { get; set; }
    }
}