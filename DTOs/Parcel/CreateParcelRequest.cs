using System.ComponentModel.DataAnnotations;

namespace MTSS.DTOs.Parcel
{
    public class CreateParcelRequest
    {
        [Range(1, int.MaxValue)]
        public int FlatId { get; set; }

        [Range(1, int.MaxValue)]
        public int ResidentProfileId { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string CourierName { get; set; } = string.Empty;

        [StringLength(100)]
        public string TrackingNumber { get; set; } = string.Empty;
    }
}