namespace MTSS.DTOs.Parcel
{
    public class ParcelResponse
    {
        public int ParcelId { get; set; }
        public int SocietyId { get; set; }
        public int FlatId { get; set; }
        public int ResidentProfileId { get; set; }
        public string CourierName { get; set; } = string.Empty;
        public string TrackingNumber { get; set; } = string.Empty;
        public DateTime ReceivedAt { get; set; }
        public DateTime? CollectedAt { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}