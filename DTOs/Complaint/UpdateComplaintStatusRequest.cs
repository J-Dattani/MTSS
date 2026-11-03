
using System.ComponentModel.DataAnnotations;

namespace MTSS.DTOs.Complaint
{
    public class UpdateComplaintStatusRequest
    {
        [Required]
        [RegularExpression(
            "^(InProgress|Resolved|Rejected)$",
            ErrorMessage = "Status must be InProgress, Resolved, or Rejected."
        )]
        public string Status { get; set; } = string.Empty;

        [StringLength(500)]
        public string? AdminRemarks { get; set; }
    }
}
