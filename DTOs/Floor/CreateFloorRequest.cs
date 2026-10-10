
using System.ComponentModel.DataAnnotations;

namespace MTSS.DTOs.Floor
{
    public class CreateFloorRequest
    {
        [Range(1, int.MaxValue)]
        public int WingId { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Name { get; set; } = string.Empty;

        [Range(0, int.MaxValue)]
        public int FloorNumber { get; set; }
    }
}
