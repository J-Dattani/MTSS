namespace MTSS.Models
{
    public class SocietySubscription
    {
        public int SocietySubscriptionId { get; set; }

        public int SocietyId { get; set; }

        public int SubscriptionPlanId { get; set; }

        public string Status { get; set; } = "Trial";

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}