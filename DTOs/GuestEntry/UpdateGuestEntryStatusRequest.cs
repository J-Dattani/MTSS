
using System.ComponentModel.DataAnnotations;

namespace MTSS.DTOs.GuestEntry
{
    public class UpdateGuestEntryStatusRequest
    {
        [Required]
        [RegularExpression("^(Approved|Rejected)$",
            ErrorMessage = "Status must be Approved or Rejected.")]
        public string Status { get; set; } = string.Empty;
    }
}
