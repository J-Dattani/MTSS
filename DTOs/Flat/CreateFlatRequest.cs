
using System.ComponentModel.DataAnnotations;

namespace MTSS.DTOs.Flat
{
    public class CreateFlatRequest
    {
        [Range(1, int.MaxValue)]
        public int FloorId { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 1)]
        public string FlatNumber { get; set; } = string.Empty;
    }
}
