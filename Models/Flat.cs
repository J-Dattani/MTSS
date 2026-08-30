namespace MTSS.Models
{
    public class Flat
    {
        public int FlatId { get; set; }

        public int SocietyId { get; set; }

        public int WingId { get; set; }

        public int FloorId { get; set; }

        public string FlatNumber { get; set; } = string.Empty;

        public bool IsOccupied { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}