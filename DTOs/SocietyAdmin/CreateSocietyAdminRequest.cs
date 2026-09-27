using System.ComponentModel.DataAnnotations;

namespace MTSS.DTOs.SocietyAdmin
{
    public class CreateSocietyAdminRequest
    {
        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}