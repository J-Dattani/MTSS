
using System.ComponentModel.DataAnnotations;

namespace MTSS.DTOs.Wing
{
    public class UpdateWingRequest
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Name { get; set; } = string.Empty;
    }
}
