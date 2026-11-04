
namespace MTSS.DTOs.Reports
{
    public class AmenityAccessReportResponse
    {
        public int AmenityAccessLogId { get; set; }
        public int AmenityId { get; set; }
        public string AmenityName { get; set; } = string.Empty;
        public int FlatId { get; set; }
        public int ResidentProfileId { get; set; }
        public bool IsSuccessful { get; set; }
        public DateTime AttemptedAt { get; set; }
        public string? FailureReason { get; set; }
    }
}
