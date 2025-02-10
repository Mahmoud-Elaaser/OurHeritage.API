using System.ComponentModel.DataAnnotations;

namespace OurHeritage.Service.DTOs.AuthDto
{
    public class LoginDto
    {
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }


    }
}
