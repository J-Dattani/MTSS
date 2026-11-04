
namespace MTSS.DTOs.Reports
{
    public class ParcelReportResponse
    {
        public int ParcelId { get; set; }
        public int FlatId { get; set; }
        public int ResidentProfileId { get; set; }
        public string CourierName { get; set; } = string.Empty;
        public string? TrackingNumber { get; set; }
        public DateTime ReceivedAt { get; set; }
        public DateTime? CollectedAt { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
