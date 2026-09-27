namespace MTSS.DTOs.SocietyAdmin
{
    public class SocietyAdminResponse
    {
        public string UserId { get; set; } = string.Empty;

        public int SocietyId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public bool IsActive { get; set; }
    }
}