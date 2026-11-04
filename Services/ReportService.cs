
using Microsoft.EntityFrameworkCore;
using MTSS.Data;
using MTSS.DTOs.Reports;

namespace MTSS.Services
{
    public class ReportService
    {
        private readonly ApplicationDbContext _context;

        public ReportService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get Society Dashboard Summary
        public async Task<SocietyDashboardResponse> GetDashboardAsync(
            int societyId)
        {
            var society = await _context.Societies
                .AsNoTracking()
                .FirstOrDefaultAsync(s =>
                    s.SocietyId == societyId && s.IsActive);

            if (society == null)
                throw new KeyNotFoundException(
                    "Active society not found.");

            var result = new SocietyDashboardResponse
            {
                SocietyName = society.Name,

                // Infrastructure
                TotalWings = await _context.Wings.CountAsync(w =>
                    w.SocietyId == societyId && w.IsActive),

                TotalFloors = await _context.Floors.CountAsync(f =>
                    f.SocietyId == societyId && f.IsActive),

                TotalFlats = await _context.Flats.CountAsync(f =>
                    f.SocietyId == societyId && f.IsActive),

                OccupiedFlats = await _context.Flats.CountAsync(f =>
                    f.SocietyId == societyId &&
                    f.IsActive && f.IsOccupied),

                VacantFlats = await _context.Flats.CountAsync(f =>
                    f.SocietyId == societyId &&
                    f.IsActive && !f.IsOccupied),

                // Residents
                TotalResidents = await _context.ResidentProfiles.CountAsync(
                    r => r.SocietyId == societyId),

                ActiveResidents = await _context.ResidentProfiles.CountAsync(
                    r => r.SocietyId == societyId && r.IsActive),

                InactiveResidents = await _context.ResidentProfiles.CountAsync(
                    r => r.SocietyId == societyId && !r.IsActive),

                ResidentsAtHome = await _context.ResidentProfiles.CountAsync(
                    r => r.SocietyId == societyId &&
                         r.IsActive && r.IsHome),

                // Amenities
                TotalAmenities = await _context.Amenities.CountAsync(a =>
                    a.SocietyId == societyId && a.IsActive),

                // Guest Entries
                TotalGuestEntries = await _context.GuestEntryRequests.CountAsync(
                    g => g.SocietyId == societyId),

                PendingGuestEntries = await _context.GuestEntryRequests.CountAsync(
                    g => g.SocietyId == societyId &&
                         g.Status == "Pending"),

                ApprovedGuestEntries = await _context.GuestEntryRequests.CountAsync(
                    g => g.SocietyId == societyId &&
                         g.Status == "Approved"),

                RejectedGuestEntries = await _context.GuestEntryRequests.CountAsync(
                    g => g.SocietyId == societyId &&
                         g.Status == "Rejected"),

                // Parcels
                TotalParcels = await _context.Parcels.CountAsync(
                    p => p.SocietyId == societyId),

                ReceivedParcels = await _context.Parcels.CountAsync(
                    p => p.SocietyId == societyId &&
                         p.Status == "Received"),

                CollectedParcels = await _context.Parcels.CountAsync(
                    p => p.SocietyId == societyId &&
                         p.Status == "Collected"),

                // Complaints
                TotalComplaints = await _context.Complaints.CountAsync(
                    c => c.SocietyId == societyId),

                OpenComplaints = await _context.Complaints.CountAsync(
                    c => c.SocietyId == societyId &&
                         c.Status == "Open"),

                InProgressComplaints = await _context.Complaints.CountAsync(
                    c => c.SocietyId == societyId &&
                         c.Status == "InProgress"),

                ResolvedComplaints = await _context.Complaints.CountAsync(
                    c => c.SocietyId == societyId &&
                         c.Status == "Resolved"),

                RejectedComplaints = await _context.Complaints.CountAsync(
                    c => c.SocietyId == societyId &&
                         c.Status == "Rejected"),

                // Amenity Access
                TotalAccessAttempts = await _context.AmenityAccessLogs.CountAsync(
                    l => l.SocietyId == societyId),

                SuccessfulAccessAttempts = await _context.AmenityAccessLogs.CountAsync(
                    l => l.SocietyId == societyId &&
                         l.IsSuccessful),

                FailedAccessAttempts = await _context.AmenityAccessLogs.CountAsync(
                    l => l.SocietyId == societyId &&
                         !l.IsSuccessful)
            };

            return result;
        }

        // Get Society Resident Report
        public async Task<List<ResidentReportResponse>> GetResidentsReportAsync(
            int societyId)
        {
            var societyExists = await _context.Societies
                .AnyAsync(s =>
                    s.SocietyId == societyId && s.IsActive);

            if (!societyExists)
                throw new KeyNotFoundException(
                    "Active society not found.");

            return await _context.ResidentProfiles
                .AsNoTracking()
                .Where(r => r.SocietyId == societyId)
                .OrderBy(r => r.FullName)
                .Select(r => new ResidentReportResponse
                {
                    ResidentProfileId = r.ResidentProfileId,
                    FullName = r.FullName,
                    MobileNumber = r.MobileNumber,
                    FlatId = r.FlatId,
                    IsHome = r.IsHome,
                    IsActive = r.IsActive,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();
        }

        // Get Society Guest Entry Report
        public async Task<List<GuestEntryReportResponse>> GetGuestEntriesReportAsync(
            int societyId)
        {
            var societyExists = await _context.Societies
                .AnyAsync(s =>
                    s.SocietyId == societyId && s.IsActive);

            if (!societyExists)
                throw new KeyNotFoundException(
                    "Active society not found.");

            return await _context.GuestEntryRequests
                .AsNoTracking()
                .Where(g => g.SocietyId == societyId)
                .OrderByDescending(g => g.CreatedAt)
                .Select(g => new GuestEntryReportResponse
                {
                    GuestEntryRequestId = g.GuestEntryRequestId,
                    FlatId = g.FlatId,
                    ResidentProfileId = g.ResidentProfileId,
                    GuestName = g.GuestName,
                    GuestPhone = g.GuestPhone,
                    Purpose = g.Purpose,
                    Status = g.Status,
                    CreatedAt = g.CreatedAt,
                    ResidentRespondedAt = g.ResidentRespondedAt,
                    AllowedEntryAt = g.AllowedEntryAt
                })
                .ToListAsync();
        }

        // Get Society Parcel Report
        public async Task<List<ParcelReportResponse>> GetParcelsReportAsync(
            int societyId)
        {
            var societyExists = await _context.Societies
                .AnyAsync(s =>
                    s.SocietyId == societyId && s.IsActive);

            if (!societyExists)
                throw new KeyNotFoundException(
                    "Active society not found.");

            return await _context.Parcels
                .AsNoTracking()
                .Where(p => p.SocietyId == societyId)
                .OrderByDescending(p => p.ReceivedAt)
                .Select(p => new ParcelReportResponse
                {
                    ParcelId = p.ParcelId,
                    FlatId = p.FlatId,
                    ResidentProfileId = p.ResidentProfileId,
                    CourierName = p.CourierName,
                    TrackingNumber = p.TrackingNumber,
                    ReceivedAt = p.ReceivedAt,
                    CollectedAt = p.CollectedAt,
                    Status = p.Status
                })
                .ToListAsync();
        }

        // Get Society Complaint Report
        public async Task<List<ComplaintReportResponse>> GetComplaintsReportAsync(
            int societyId)
        {
            var societyExists = await _context.Societies
                .AnyAsync(s =>
                    s.SocietyId == societyId && s.IsActive);

            if (!societyExists)
                throw new KeyNotFoundException(
                    "Active society not found.");

            return await _context.Complaints
                .AsNoTracking()
                .Where(c => c.SocietyId == societyId)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new ComplaintReportResponse
                {
                    ComplaintId = c.ComplaintId,
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

        // Get Society Amenity Access Report
        public async Task<List<AmenityAccessReportResponse>> GetAmenityAccessReportAsync(
            int societyId)
        {
            var societyExists = await _context.Societies
                .AnyAsync(s =>
                    s.SocietyId == societyId && s.IsActive);

            if (!societyExists)
                throw new KeyNotFoundException(
                    "Active society not found.");

            return await (
                from log in _context.AmenityAccessLogs.AsNoTracking()
                join amenity in _context.Amenities.AsNoTracking()
                    on log.AmenityId equals amenity.AmenityId
                where log.SocietyId == societyId
                orderby log.AttemptedAt descending
                select new AmenityAccessReportResponse
                {
                    AmenityAccessLogId = log.AmenityAccessLogId,
                    AmenityId = log.AmenityId,
                    AmenityName = amenity.Name,
                    FlatId = log.FlatId,
                    ResidentProfileId = log.ResidentProfileId,
                    IsSuccessful = log.IsSuccessful,
                    AttemptedAt = log.AttemptedAt,
                    FailureReason = log.FailureReason
                }
            ).ToListAsync();
        }
    }
}
