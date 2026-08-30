namespace MTSS.Models
{
    public class Complaint
    {
        public int ComplaintId { get; set; }

        public int SocietyId { get; set; }

        public int FlatId { get; set; }

        public int ResidentProfileId { get; set; }

        public string Subject { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Status { get; set; } = "Open";

        public string? AdminRemarks { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public DateTime? ResolvedAt { get; set; }
    }
}