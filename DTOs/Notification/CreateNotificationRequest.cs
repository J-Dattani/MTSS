
using System.ComponentModel.DataAnnotations;

namespace MTSS.DTOs.Notification
{
    public class CreateNotificationRequest
    {
        [Required]
        [StringLength(150, MinimumLength = 3)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(1000, MinimumLength = 5)]
        public string Message { get; set; } = string.Empty;

        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Type { get; set; } = string.Empty;

        // Optional: null or empty means all active residents
        public string? UserId { get; set; }
    }
}
