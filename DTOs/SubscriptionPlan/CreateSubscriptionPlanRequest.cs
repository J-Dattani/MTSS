using System.ComponentModel.DataAnnotations;

namespace MTSS.DTOs.SubscriptionPlan
{
    public class CreateSubscriptionPlanRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Range(0, int.MaxValue)]
        public int MaxWings { get; set; }

        [Range(0, int.MaxValue)]
        public int MaxFloorsPerWing { get; set; }

        [Range(0, int.MaxValue)]
        public int MaxFlats { get; set; }

        [Range(0, int.MaxValue)]
        public int MaxAmenities { get; set; }

        [Range(0, int.MaxValue)]
        public int MaxSocietyAdmins { get; set; }

        public bool GuestApprovalEnabled { get; set; }

        public bool ParcelRegisterEnabled { get; set; }

        public bool AmenityPinEnabled { get; set; }

        public bool ComplaintManagementEnabled { get; set; }

        public bool ReportsEnabled { get; set; }
    }
}