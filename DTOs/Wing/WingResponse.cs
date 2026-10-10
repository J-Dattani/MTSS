namespace MTSS.DTOs.Wing
{
    public class WingResponse
    {
        public int WingId { get; set; }
        public int SocietyId { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}