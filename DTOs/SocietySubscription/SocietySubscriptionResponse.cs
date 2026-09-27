namespace MTSS.DTOs.SocietySubscription
{
    public class SocietySubscriptionResponse
    {
        public int SocietySubscriptionId { get; set; }

        public int SocietyId { get; set; }

        public int SubscriptionPlanId { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}