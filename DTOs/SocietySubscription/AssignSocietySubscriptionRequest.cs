using System.ComponentModel.DataAnnotations;

namespace MTSS.DTOs.SocietySubscription
{
    public class AssignSocietySubscriptionRequest
    {
        [Range(1, int.MaxValue)]
        public int SubscriptionPlanId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }
    }
}