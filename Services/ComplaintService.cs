
using Microsoft.EntityFrameworkCore;
using MTSS.Data;
using MTSS.DTOs.Complaint;
using MTSS.Models;

namespace MTSS.Services
{
    public class ComplaintService
    {
        private readonly ApplicationDbContext _context;

        public ComplaintService(ApplicationDbContext context)
        {
            _context = context;
        }

        private async Task ValidateSocietyAccessAsync(int societyId)
        {
            var societyExists = await _context.Societies
                .AnyAsync(s => s.SocietyId == societyId && s.IsActive);

            if (!societyExists)
                throw new KeyNotFoundException("Active society not found.");

            var today = DateTime.UtcNow.Date;

            var subscription = await _context.SocietySubscriptions
                .Where(s =>
                    s.SocietyId == societyId &&
                    s.IsActive &&
                    s.StartDate <= today &&
                    s.EndDate >= today &&
                    (s.Status == "Active" || s.Status == "Trial"))
                .OrderByDescending(s => s.SocietySubscriptionId)
                .FirstOrDefaultAsync();

            if (subscription == null)
                throw new InvalidOperationException(
                    "No valid active subscription found for this society.");

            var plan = await _context.SubscriptionPlans
                .FirstOrDefaultAsync(p =>
                    p.SubscriptionPlanId == subscription.SubscriptionPlanId &&
                    p.IsActive);

            if (plan == null || !plan.ComplaintManagementEnabled)
                throw new InvalidOperationException(
                    "Complaint management is not enabled for this society.");
        }

        private async Task<ResidentProfile> GetLinkedResidentAsync(
            string applicationUserId)
        {
            var resident = await _context.ResidentProfiles
                .Include(r => r.ApplicationUser)
                .FirstOrDefaultAsync(r =>
                    r.ApplicationUserId == applicationUserId);

            if (resident == null)
                throw new KeyNotFoundException(
                    "No resident profile is linked to this account.");

            if (!resident.IsActive)
                throw new UnauthorizedAccessException(
                    "Resident profile is inactive.");

            if (resident.ApplicationUser == null ||
                !resident.ApplicationUser.IsActive ||
                resident.ApplicationUser.SocietyId != resident.SocietyId)
            {
                throw new UnauthorizedAccessException(
                    "Resident account is inactive or invalid.");
            }

            return resident;
        }

        private async Task ValidateActiveFlatAsync(
            int societyId,
            int flatId)
        {
            var flat = await _context.Flats
                .FirstOrDefaultAsync(f =>
                    f.FlatId == flatId &&
                    f.SocietyId == societyId &&
                    f.IsActive);

            if (flat == null)
                throw new InvalidOperationException(
                    "Resident's flat is inactive or unavailable.");

            var floor = await _context.Floors
                .FirstOrDefaultAsync(f =>
                    f.FloorId == flat.FloorId &&
                    f.SocietyId == societyId &&
                    f.IsActive);

            if (floor == null)
                throw new InvalidOperationException(
                    "Resident's floor is inactive or unavailable.");

            var wingExists = await _context.Wings.AnyAsync(w =>
                w.WingId == floor.WingId &&
                w.SocietyId == societyId &&
                w.IsActive);

            if (!wingExists)
                throw new InvalidOperationException(
                    "Resident's wing is inactive or unavailable.");
        }

        // SOCIETY ADMIN: Get all complaints
        public async Task<List<ComplaintResponse>> GetAllAsync(int societyId)
        {
            await ValidateSocietyAccessAsync(societyId);

            return await _context.Complaints
                .AsNoTracking()
                .Where(c => c.SocietyId == societyId)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new ComplaintResponse
                {
                    ComplaintId = c.ComplaintId,
                    SocietyId = c.SocietyId,
                    FlatId = c.FlatId,
                    ResidentProfileId = c.ResidentProfileId,
                    Subject = c.Subject,
                    Description = c.Description,
                    Status = c.Status,
                    AdminRemarks = c.AdminRemarks,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    ResolvedAt = c.ResolvedAt
                })
                .ToListAsync();
        }

        // SOCIETY ADMIN: Get complaint by ID
        public async Task<ComplaintResponse> GetByIdAsync(
            int societyId,
            int complaintId)
        {
            await ValidateSocietyAccessAsync(societyId);

            var complaint = await _context.Complaints
                .AsNoTracking()
                .FirstOrDefaultAsync(c =>
                    c.ComplaintId == complaintId &&
                    c.SocietyId == societyId);

            if (complaint == null)
                throw new KeyNotFoundException("Complaint not found.");

            return MapResponse(complaint);
        }

        // RESIDENT: Create complaint using authenticated account
        public async Task<ComplaintResponse> CreateForResidentAsync(
            string applicationUserId,
            CreateComplaintRequest request)
        {
            var resident = await GetLinkedResidentAsync(applicationUserId);

            await ValidateSocietyAccessAsync(resident.SocietyId);

            await ValidateActiveFlatAsync(
                resident.SocietyId,
                resident.FlatId);

            var complaint = new Complaint
            {
                SocietyId = resident.SocietyId,
                FlatId = resident.FlatId,
                ResidentProfileId = resident.ResidentProfileId,
                Subject = request.Subject.Trim(),
                Description = request.Description.Trim(),
                Status = "Open",
                CreatedAt = DateTime.UtcNow
            };

            _context.Complaints.Add(complaint);
            await _context.SaveChangesAsync();

            return MapResponse(complaint);
        }

        // RESIDENT: Get only their own complaints
        public async Task<List<ComplaintResponse>> GetResidentComplaintsAsync(
            string applicationUserId)
        {
            var resident = await GetLinkedResidentAsync(applicationUserId);

            await ValidateSocietyAccessAsync(resident.SocietyId);

            return await _context.Complaints
                .AsNoTracking()
                .Where(c =>
                    c.SocietyId == resident.SocietyId &&
                    c.ResidentProfileId == resident.ResidentProfileId)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new ComplaintResponse
                {
                    ComplaintId = c.ComplaintId,
                    SocietyId = c.SocietyId,
                    FlatId = c.FlatId,
                    ResidentProfileId = c.ResidentProfileId,
                    Subject = c.Subject,
                    Description = c.Description,
                    Status = c.Status,
                    AdminRemarks = c.AdminRemarks,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    ResolvedAt = c.ResolvedAt
                })
                .ToListAsync();
        }

        // RESIDENT: Get only their own complaint by ID
        public async Task<ComplaintResponse> GetResidentComplaintByIdAsync(
            string applicationUserId,
            int complaintId)
        {
            var resident = await GetLinkedResidentAsync(applicationUserId);

            await ValidateSocietyAccessAsync(resident.SocietyId);

            var complaint = await _context.Complaints
                .AsNoTracking()
                .FirstOrDefaultAsync(c =>
                    c.ComplaintId == complaintId &&
                    c.SocietyId == resident.SocietyId &&
                    c.ResidentProfileId == resident.ResidentProfileId);

            if (complaint == null)
                throw new KeyNotFoundException("Complaint not found.");

            return MapResponse(complaint);
        }

        // SOCIETY ADMIN: Update complaint status
        public async Task<ComplaintResponse> UpdateStatusAsync(
            int societyId,
            int complaintId,
            UpdateComplaintStatusRequest request)
        {
            await ValidateSocietyAccessAsync(societyId);

            var complaint = await _context.Complaints
                .FirstOrDefaultAsync(c =>
                    c.ComplaintId == complaintId &&
                    c.SocietyId == societyId);

            if (complaint == null)
                throw new KeyNotFoundException("Complaint not found.");

            if (complaint.Status == "Resolved" ||
                complaint.Status == "Rejected")
            {
                throw new InvalidOperationException(
                    "Resolved or rejected complaints cannot be updated.");
            }

            if (complaint.Status != "Open" &&
                complaint.Status != "InProgress")
            {
                throw new InvalidOperationException(
                    "Invalid complaint status transition.");
            }

            complaint.Status = request.Status;
            complaint.AdminRemarks = request.AdminRemarks?.Trim();
            complaint.UpdatedAt = DateTime.UtcNow;

            complaint.ResolvedAt = request.Status == "Resolved"
                ? DateTime.UtcNow
                : null;

            await _context.SaveChangesAsync();

            return MapResponse(complaint);
        }

        private static ComplaintResponse MapResponse(Complaint complaint)
        {
            return new ComplaintResponse
            {
                ComplaintId = complaint.ComplaintId,
                SocietyId = complaint.SocietyId,
                FlatId = complaint.FlatId,
                ResidentProfileId = complaint.ResidentProfileId,
                Subject = complaint.Subject,
                Description = complaint.Description,
                Status = complaint.Status,
                AdminRemarks = complaint.AdminRemarks,
                CreatedAt = complaint.CreatedAt,
                UpdatedAt = complaint.UpdatedAt,
                ResolvedAt = complaint.ResolvedAt
            };
        }
    }
}
