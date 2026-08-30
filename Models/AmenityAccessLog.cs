namespace MTSS.Models
{
    public class AmenityAccessLog
    {
        public int AmenityAccessLogId { get; set; }

        public int SocietyId { get; set; }

        public int AmenityId { get; set; }

        public int FlatId { get; set; }

        public int ResidentProfileId { get; set; }

        public bool IsSuccessful { get; set; }

        public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;

        public string? FailureReason { get; set; }
    }
}