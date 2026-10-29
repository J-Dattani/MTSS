using System.ComponentModel.DataAnnotations;

namespace MTSS.DTOs.Parcel
{
    public class UpdateParcelStatusRequest
    {
        [Required]
        [RegularExpression(
            "^(Received|Collected)$",
            ErrorMessage = "Status must be Received or Collected."
        )]
        public string Status { get; set; } = string.Empty;
    }
}