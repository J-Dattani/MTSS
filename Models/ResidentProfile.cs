namespace MTSS.Models
{
    public class ResidentProfile
    {
        public int ResidentProfileId { get; set; }

        public int SocietyId { get; set; }

        public int FlatId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string MobileNumber { get; set; } = string.Empty;

        public bool IsHome { get; set; } = true;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}