
namespace MTSS.DTOs.AmenityAccessPin
{
    public class AmenityPinValidationResponse
    {
        public bool IsSuccessful { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? AmenityAccessLogId { get; set; }
        public DateTime AttemptedAt { get; set; }
    }
}
