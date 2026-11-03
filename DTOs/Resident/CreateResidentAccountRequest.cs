using System.ComponentModel.DataAnnotations;

namespace MTSS.DTOs.Resident
{
    public class CreateResidentAccountRequest
    {
        [Required]
        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Password { get; set; } = string.Empty;
    }
}