using System.ComponentModel.DataAnnotations;

namespace MTSS.DTOs.Complaint
{
    public class CreateComplaintRequest
    {
        [Required]
        [StringLength(150, MinimumLength = 3)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        [StringLength(1000, MinimumLength = 5)]
        public string Description { get; set; } = string.Empty;
    }
}