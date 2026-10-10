
namespace MTSS.DTOs.Resident
{
    public class ResidentResponse
    {
        public int ResidentProfileId { get; set; }

        public int SocietyId { get; set; }

        public int FlatId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string MobileNumber { get; set; } = string.Empty;

        public bool IsHome { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
