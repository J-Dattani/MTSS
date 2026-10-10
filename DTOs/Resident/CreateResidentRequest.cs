
using System.ComponentModel.DataAnnotations;

namespace MTSS.DTOs.Resident
{
    public class CreateResidentRequest
    {
        [Range(1, int.MaxValue)]
        public int FlatId { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [Phone]
        [StringLength(15, MinimumLength = 7)]
        public string MobileNumber { get; set; } = string.Empty;
    }
}
