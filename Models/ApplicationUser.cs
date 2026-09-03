using Microsoft.AspNetCore.Identity;

namespace MTSS.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;

        public int? SocietyId { get; set; }

        public bool IsActive { get; set; } = true;
    }
}