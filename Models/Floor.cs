namespace MTSS.Models
{
    public class Floor
    {
        public int FloorId { get; set; }

        public int SocietyId { get; set; }

        public int WingId { get; set; }

        public string Name { get; set; } = string.Empty;

        public int FloorNumber { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}