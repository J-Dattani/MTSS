
using System.ComponentModel.DataAnnotations;

namespace MTSS.DTOs.GuestEntry
{
    public class CreateGuestEntryRequest
    {
        [Range(1, int.MaxValue)]
        public int FlatId { get; set; }

        [Range(1, int.MaxValue)]
        public int ResidentProfileId { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string GuestName { get; set; } = string.Empty;

        [Required]
        [Phone]
        [StringLength(15, MinimumLength = 7)]
        public string GuestPhone { get; set; } = string.Empty;

        [Required]
        [StringLength(300, MinimumLength = 2)]
        public string Purpose { get; set; } = string.Empty;
    }
}
