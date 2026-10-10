
namespace MTSS.DTOs.Flat
{
    public class FlatResponse
    {
        public int FlatId { get; set; }
        public int SocietyId { get; set; }
        public int WingId { get; set; }
        public int FloorId { get; set; }
        public string FlatNumber { get; set; } = string.Empty;
        public bool IsOccupied { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
