
namespace MTSS.DTOs.Reports
{
    public class ComplaintReportResponse
    {
        public int ComplaintId { get; set; }
        public int FlatId { get; set; }
        public int ResidentProfileId { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? AdminRemarks { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
    }
}
