
using System.ComponentModel.DataAnnotations;

namespace MTSS.DTOs.AmenityAccessPin
{
    public class GenerateAmenityPinRequest
    {
        [Required]
        public int AmenityId { get; set; }
    }
}
