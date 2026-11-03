using System.ComponentModel.DataAnnotations;

namespace MTSS.DTOs.Resident
{
    public class LinkResidentAccountRequest
    {
        [Required]
        public string ApplicationUserId { get; set; } = string.Empty;
    }
}