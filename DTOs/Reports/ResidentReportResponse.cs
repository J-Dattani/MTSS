
namespace MTSS.DTOs.Reports
{
    public class ResidentReportResponse
    {
        public int ResidentProfileId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;
        public int FlatId { get; set; }
        public bool IsHome { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
