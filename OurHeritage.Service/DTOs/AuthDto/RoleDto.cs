using System.ComponentModel.DataAnnotations;

namespace OurHeritage.Service.DTOs.AuthDto
{
    public class RoleDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public string RoleName { get; set; }
    }
}
