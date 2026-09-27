using System.ComponentModel.DataAnnotations;

namespace MTSS.DTOs.SocietySubscription
{
    public class UpdateSocietySubscriptionStatusRequest
    {
        [Required]
        public string Status { get; set; } = string.Empty;

        public bool IsActive { get; set; }
    }
}