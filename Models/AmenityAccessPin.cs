namespace MTSS.Models
{
    public class AmenityAccessPin
    {
        public int AmenityAccessPinId { get; set; }

        public int SocietyId { get; set; }

        public int AmenityId { get; set; }

        public int FlatId { get; set; }

        public int ResidentProfileId { get; set; }

        public string PinHash { get; set; } = string.Empty;

        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        public DateTime ExpiresAt { get; set; }

        public DateTime? UsedAt { get; set; }

        public string Status { get; set; } = "Active";
    }
}