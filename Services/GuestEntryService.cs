
using Microsoft.EntityFrameworkCore;
using MTSS.Data;
using MTSS.DTOs.GuestEntry;
using MTSS.Models;

namespace MTSS.Services
{
    public class GuestEntryService
    {
        private readonly ApplicationDbContext _context;

        public GuestEntryService(ApplicationDbContext context)
        {
            _context = context;
        }

        private async Task ValidateSocietyAccessAsync(int societyId)
        {
            var societyExists = await _context.Societies.AnyAsync(s =>
                s.SocietyId == societyId && s.IsActive);

            if (!societyExists)
                throw new UnauthorizedAccessException(
                    "Society is inactive or unavailable.");

            var now = DateTime.UtcNow;

            var subscription = await _context.SocietySubscriptions
                .Include(s => s.SubscriptionPlan)
                .Where(s =>
                    s.SocietyId == societyId &&
                    s.IsActive &&
                    s.StartDate <= now &&
                    s.EndDate > now &&
                    (s.Status == "Active" || s.Status == "Trial") &&
                    s.SubscriptionPlan != null &&
                    s.SubscriptionPlan.IsActive)
                .OrderByDescending(s => s.SocietySubscriptionId)
                .FirstOrDefaultAsync();

            if (subscription?.SubscriptionPlan == null)
                throw new InvalidOperationException(
                    "Society does not have a valid active subscription.");

            if (!subscription.SubscriptionPlan.GuestApprovalEnabled)
                throw new InvalidOperationException(
                    "Guest entry management is not enabled for this subscription plan.");
        }

        public async Task<List<GuestEntryResponse>> GetAllAsync(int societyId)
        {
            await ValidateSocietyAccessAsync(societyId);

            return await _context.GuestEntryRequests
                .Where(g => g.SocietyId == societyId)
                .OrderByDescending(g => g.CreatedAt)
                .Select(g => new GuestEntryResponse
                {
                    GuestEntryRequestId = g.GuestEntryRequestId,
                    SocietyId = g.SocietyId,
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

        public async Task<GuestEntryResponse> GetByIdAsync(
            int societyId, int guestEntryRequestId)
        {
            await ValidateSocietyAccessAsync(societyId);

            var guestRequest = await _context.GuestEntryRequests
                .FirstOrDefaultAsync(g =>
                    g.GuestEntryRequestId == guestEntryRequestId &&
                    g.SocietyId == societyId);

            if (guestRequest == null)
                throw new KeyNotFoundException(
                    "Guest entry request not found in this society.");

            return MapResponse(guestRequest);
        }

        public async Task<GuestEntryResponse> CreateAsync(
            int societyId, CreateGuestEntryRequest request)
        {
            await ValidateSocietyAccessAsync(societyId);

            var flat = await _context.Flats.FirstOrDefaultAsync(f =>
                f.FlatId == request.FlatId &&
                f.SocietyId == societyId &&
                f.IsActive);

            if (flat == null)
                throw new KeyNotFoundException(
                    "Active flat not found in this society.");

            var activeFloorExists = await _context.Floors.AnyAsync(f =>
                f.FloorId == flat.FloorId &&
                f.SocietyId == societyId &&
                f.IsActive &&
                _context.Wings.Any(w =>
                    w.WingId == f.WingId &&
                    w.SocietyId == societyId &&
                    w.IsActive));

            if (!activeFloorExists)
                throw new InvalidOperationException(
                    "The flat's floor or wing is inactive or unavailable.");

            var resident = await _context.ResidentProfiles.FirstOrDefaultAsync(r =>
                r.ResidentProfileId == request.ResidentProfileId &&
                r.SocietyId == societyId &&
                r.FlatId == flat.FlatId &&
                r.IsActive);

            if (resident == null)
                throw new KeyNotFoundException(
                    "Active resident not found for the selected flat and society.");

            var guestName = request.GuestName.Trim();
            var guestPhone = request.GuestPhone.Trim();
            var purpose = request.Purpose.Trim();

            if (string.IsNullOrWhiteSpace(guestName) ||
                string.IsNullOrWhiteSpace(guestPhone) ||
                string.IsNullOrWhiteSpace(purpose))
            {
                throw new ArgumentException(
                    "Guest name, phone, and purpose are required.");
            }

            var guestRequest = new GuestEntryRequest
            {
                SocietyId = societyId,
                FlatId = flat.FlatId,
                ResidentProfileId = resident.ResidentProfileId,
                GuestName = guestName,
                GuestPhone = guestPhone,
                Purpose = purpose,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                ResidentRespondedAt = null,
                AllowedEntryAt = null
            };

            _context.GuestEntryRequests.Add(guestRequest);
            await _context.SaveChangesAsync();

            return MapResponse(guestRequest);
        }

        public async Task<GuestEntryStatusResponse> UpdateStatusAsync(
            int societyId,
            int guestEntryRequestId,
            UpdateGuestEntryStatusRequest request)
        {
            await ValidateSocietyAccessAsync(societyId);

            var guestRequest = await _context.GuestEntryRequests
                .FirstOrDefaultAsync(g =>
                    g.GuestEntryRequestId == guestEntryRequestId &&
                    g.SocietyId == societyId);

            if (guestRequest == null)
                throw new KeyNotFoundException(
                    "Guest entry request not found in this society.");

            if (guestRequest.Status != "Pending")
                throw new InvalidOperationException(
                    "Only pending guest entry requests can be approved or rejected.");

            if (request.Status != "Approved" &&
                request.Status != "Rejected")
            {
                throw new ArgumentException(
                    "Status must be Approved or Rejected.");
            }

            guestRequest.Status = request.Status;

            // This is an administrative decision, not a resident response.
            // Do not set ResidentRespondedAt here.
            guestRequest.AllowedEntryAt =
                request.Status == "Approved" ? DateTime.UtcNow : null;

            await _context.SaveChangesAsync();

            return new GuestEntryStatusResponse
            {
                GuestEntryRequestId = guestRequest.GuestEntryRequestId,
                Status = guestRequest.Status,
                ResidentRespondedAt = guestRequest.ResidentRespondedAt,
                AllowedEntryAt = guestRequest.AllowedEntryAt
            };
        }

        private static GuestEntryResponse MapResponse(
            GuestEntryRequest guestRequest)
        {
            return new GuestEntryResponse
            {
                GuestEntryRequestId = guestRequest.GuestEntryRequestId,
                SocietyId = guestRequest.SocietyId,
                FlatId = guestRequest.FlatId,
                ResidentProfileId = guestRequest.ResidentProfileId,
                GuestName = guestRequest.GuestName,
                GuestPhone = guestRequest.GuestPhone,
                Purpose = guestRequest.Purpose,
                Status = guestRequest.Status,
                CreatedAt = guestRequest.CreatedAt,
                ResidentRespondedAt = guestRequest.ResidentRespondedAt,
                AllowedEntryAt = guestRequest.AllowedEntryAt
            };
        }
    }
}
