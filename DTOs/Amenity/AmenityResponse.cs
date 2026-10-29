
namespace MTSS.DTOs.Amenity
{
    public class AmenityResponse
    {
        public int AmenityId { get; set; }
        public int SocietyId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TimeSpan OpeningTime { get; set; }
        public TimeSpan ClosingTime { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
