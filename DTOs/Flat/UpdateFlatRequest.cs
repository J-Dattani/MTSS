
using System.ComponentModel.DataAnnotations;

namespace MTSS.DTOs.Flat
{
    public class UpdateFlatRequest
    {
        [Required]
        [StringLength(50, MinimumLength = 1)]
        public string FlatNumber { get; set; } = string.Empty;

        public bool IsOccupied { get; set; }
    }
}
