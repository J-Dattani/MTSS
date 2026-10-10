
namespace MTSS.DTOs.Floor
{
    public class FloorResponse
    {
        public int FloorId { get; set; }
        public int SocietyId { get; set; }
        public int WingId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int FloorNumber { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
