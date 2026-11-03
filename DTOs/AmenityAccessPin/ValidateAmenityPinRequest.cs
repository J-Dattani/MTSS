
using System.ComponentModel.DataAnnotations;

namespace MTSS.DTOs.AmenityAccessPin
{
    public class ValidateAmenityPinRequest
    {
        [Required]
        public int AmenityId { get; set; }

        [Required]
        [RegularExpression(@"^\d{6}$",
            ErrorMessage = "PIN must contain exactly 6 digits.")]
        public string Pin { get; set; } = string.Empty;
    }
}
