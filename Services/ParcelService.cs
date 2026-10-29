
using Microsoft.EntityFrameworkCore;
using MTSS.Data;
using MTSS.DTOs.Parcel;
using MTSS.Models;

namespace MTSS.Services
{
    public class ParcelService
    {
        private readonly ApplicationDbContext _context;

        public ParcelService(ApplicationDbContext context)
        {
            _context = context;
        }

        private async Task ValidateSocietyAccessAsync(int societyId)
        {
            var society = await _context.Societies
                .FirstOrDefaultAsync(s => s.SocietyId == societyId && s.IsActive);

            if (society == null)
                throw new KeyNotFoundException("Active society not found.");

            var today = DateTime.UtcNow.Date;

            var subscription = await _context.SocietySubscriptions
                .Where(s => s.SocietyId == societyId
                    && s.IsActive
                    && s.StartDate <= today
                    && s.EndDate >= today
                    && (s.Status == "Active" || s.Status == "Trial"))
                .OrderByDescending(s => s.SocietySubscriptionId)
                .FirstOrDefaultAsync();

            if (subscription == null)
                throw new InvalidOperationException(
                    "No valid active subscription found for this society.");

            var plan = await _context.SubscriptionPlans
                .FirstOrDefaultAsync(p =>
                    p.SubscriptionPlanId == subscription.SubscriptionPlanId
                    && p.IsActive);

            if (plan == null || !plan.ParcelRegisterEnabled)
                throw new InvalidOperationException(
                    "Parcel registration is not enabled for this society's subscription.");
        }

        public async Task<List<ParcelResponse>> GetAllAsync(int societyId)
        {
            await ValidateSocietyAccessAsync(societyId);

            return await _context.Parcels
                .AsNoTracking()
                .Where(p => p.SocietyId == societyId)
                .OrderByDescending(p => p.ReceivedAt)
                .Select(p => new ParcelResponse
                {
                    ParcelId = p.ParcelId,
                    SocietyId = p.SocietyId,
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

        public async Task<ParcelResponse> GetByIdAsync(int societyId, int parcelId)
        {
            await ValidateSocietyAccessAsync(societyId);

            var parcel = await _context.Parcels
                .AsNoTracking()
                .FirstOrDefaultAsync(p =>
                    p.ParcelId == parcelId && p.SocietyId == societyId);

            if (parcel == null)
                throw new KeyNotFoundException("Parcel not found.");

            return MapResponse(parcel);
        }

        public async Task<ParcelResponse> CreateAsync(
            int societyId, CreateParcelRequest request)
        {
            await ValidateSocietyAccessAsync(societyId);

            var flat = await _context.Flats
    .FirstOrDefaultAsync(f =>
        f.FlatId == request.FlatId
        && f.SocietyId == societyId
        && f.IsActive);

            if (flat == null)
                throw new ArgumentException(
                    "Active flat not found in this society.");

            var floor = await _context.Floors
                .FirstOrDefaultAsync(f =>
                    f.FloorId == flat.FloorId
                    && f.SocietyId == societyId
                    && f.IsActive);

            if (floor == null)
                throw new ArgumentException(
                    "Active floor not found for this flat.");

            var wing = await _context.Wings
                .FirstOrDefaultAsync(w =>
                    w.WingId == floor.WingId
                    && w.SocietyId == societyId
                    && w.IsActive);

            if (wing == null)
                throw new ArgumentException(
                    "Active wing not found for this flat.");

            if (flat == null)
                throw new ArgumentException(
                    "Active flat, floor, and wing not found in this society.");

            var resident = await _context.ResidentProfiles
                .FirstOrDefaultAsync(r =>
                    r.ResidentProfileId == request.ResidentProfileId
                    && r.SocietyId == societyId
                    && r.FlatId == request.FlatId
                    && r.IsActive);

            if (resident == null)
                throw new ArgumentException(
                    "Active resident not found for the selected flat and society.");

            var parcel = new Parcel
            {
                SocietyId = societyId,
                FlatId = request.FlatId,
                ResidentProfileId = request.ResidentProfileId,
                CourierName = request.CourierName.Trim(),
                TrackingNumber = request.TrackingNumber.Trim(),
                ReceivedAt = DateTime.UtcNow,
                CollectedAt = null,
                Status = "Received"
            };

            _context.Parcels.Add(parcel);
            await _context.SaveChangesAsync();

            return MapResponse(parcel);
        }

        public async Task<ParcelResponse> UpdateStatusAsync(
            int societyId, int parcelId, UpdateParcelStatusRequest request)
        {
            await ValidateSocietyAccessAsync(societyId);

            var parcel = await _context.Parcels
                .FirstOrDefaultAsync(p =>
                    p.ParcelId == parcelId && p.SocietyId == societyId);

            if (parcel == null)
                throw new KeyNotFoundException("Parcel not found.");

            if (parcel.Status == "Collected")
                throw new InvalidOperationException(
                    "Collected parcels cannot change status.");

            if (request.Status != "Collected")
                throw new InvalidOperationException(
                    "A parcel can only transition from Received to Collected.");

            parcel.Status = "Collected";
            parcel.CollectedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return MapResponse(parcel);
        }

        private static ParcelResponse MapResponse(Parcel parcel)
        {
            return new ParcelResponse
            {
                ParcelId = parcel.ParcelId,
                SocietyId = parcel.SocietyId,
                FlatId = parcel.FlatId,
                ResidentProfileId = parcel.ResidentProfileId,
                CourierName = parcel.CourierName,
                TrackingNumber = parcel.TrackingNumber,
                ReceivedAt = parcel.ReceivedAt,
                CollectedAt = parcel.CollectedAt,
                Status = parcel.Status
            };
        }
    }
}
