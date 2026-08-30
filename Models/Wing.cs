namespace MTSS.Models
{
    public class Wing
    {
        public int WingId { get; set; }

        public int SocietyId { get; set; }

        public string Name { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}