
namespace MTSS.DTOs.AmenityAccessPin
{
    public class AmenityPinResponse
    {
        public int AmenityAccessPinId { get; set; }
        public int AmenityId { get; set; }
        public string AmenityName { get; set; } = string.Empty;
        public string? Pin { get; set; }
        public DateTime GeneratedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime? UsedAt { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
