
namespace MTSS.DTOs.GuestEntry
{
    public class GuestEntryStatusResponse
    {
        public int GuestEntryRequestId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? ResidentRespondedAt { get; set; }
        public DateTime? AllowedEntryAt { get; set; }
    }
}
