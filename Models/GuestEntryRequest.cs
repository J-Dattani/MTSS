namespace MTSS.Models
{
    public class GuestEntryRequest
    {
        public int GuestEntryRequestId { get; set; }

        public int SocietyId { get; set; }

        public int FlatId { get; set; }

        public int ResidentProfileId { get; set; }

        public string GuestName { get; set; } = string.Empty;

        public string GuestPhone { get; set; } = string.Empty;

        public string Purpose { get; set; } = string.Empty;

        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ResidentRespondedAt { get; set; }

        public DateTime? AllowedEntryAt { get; set; }
    }
}