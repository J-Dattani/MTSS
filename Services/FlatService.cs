
using Microsoft.EntityFrameworkCore;
using MTSS.Data;
using MTSS.DTOs.Flat;
using MTSS.Models;

namespace MTSS.Services
{
    public class FlatService
    {
        private readonly ApplicationDbContext _context;

        public FlatService(ApplicationDbContext context)
        {
            _context = context;
        }

        private async Task<SubscriptionPlan> ValidateSocietyAccessAsync(
            int societyId)
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

            if (subscription == null || subscription.SubscriptionPlan == null)
                throw new InvalidOperationException(
                    "Society does not have a valid active subscription.");

            return subscription.SubscriptionPlan;
        }

        private async Task<Floor> GetActiveFloorAsync(
            int societyId, int floorId)
        {
            var floor = await _context.Floors.FirstOrDefaultAsync(f =>
                f.FloorId == floorId &&
                f.SocietyId == societyId &&
                f.IsActive);

            if (floor == null)
                throw new KeyNotFoundException(
                    "Active floor not found in this society.");

            var activeWingExists = await _context.Wings.AnyAsync(w =>
                w.WingId == floor.WingId &&
                w.SocietyId == societyId &&
                w.IsActive);

            if (!activeWingExists)
                throw new InvalidOperationException(
                    "The floor's wing is inactive or unavailable.");

            return floor;
        }

        public async Task<List<FlatResponse>> GetAllAsync(int societyId)
        {
            await ValidateSocietyAccessAsync(societyId);

            return await _context.Flats
                .Where(f => f.SocietyId == societyId)
                .OrderBy(f => f.FloorId)
                .ThenBy(f => f.FlatNumber)
                .Select(f => new FlatResponse
                {
                    FlatId = f.FlatId,
                    SocietyId = f.SocietyId,
                    WingId = f.WingId,
                    FloorId = f.FloorId,
                    FlatNumber = f.FlatNumber,
                    IsOccupied = f.IsOccupied,
                    IsActive = f.IsActive,
                    CreatedAt = f.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<FlatResponse> GetByIdAsync(
            int societyId, int flatId)
        {
            await ValidateSocietyAccessAsync(societyId);

            var flat = await _context.Flats.FirstOrDefaultAsync(f =>
                f.FlatId == flatId &&
                f.SocietyId == societyId);

            if (flat == null)
                throw new KeyNotFoundException(
                    "Flat not found in this society.");

            return MapResponse(flat);
        }

        public async Task<FlatResponse> CreateAsync(
            int societyId, CreateFlatRequest request)
        {
            var plan = await ValidateSocietyAccessAsync(societyId);

            var floor = await GetActiveFloorAsync(
                societyId, request.FloorId);

            var flatNumber = request.FlatNumber.Trim();

            var duplicate = await _context.Flats.AnyAsync(f =>
                f.FloorId == floor.FloorId &&
                f.FlatNumber.ToLower() == flatNumber.ToLower());

            if (duplicate)
                throw new InvalidOperationException(
                    "A flat with this number already exists on this floor.");

            var activeFlatCount = await _context.Flats.CountAsync(f =>
                f.SocietyId == societyId && f.IsActive);

            if (activeFlatCount >= plan.MaxFlats)
                throw new InvalidOperationException(
                    "The subscription plan's flat limit has been reached.");

            var flat = new Flat
            {
                SocietyId = societyId,
                WingId = floor.WingId,
                FloorId = floor.FloorId,
                FlatNumber = flatNumber,
                IsOccupied = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Flats.Add(flat);
            await _context.SaveChangesAsync();

            return MapResponse(flat);
        }

        public async Task<FlatResponse> UpdateAsync(
            int societyId, int flatId, UpdateFlatRequest request)
        {
            await ValidateSocietyAccessAsync(societyId);

            var flat = await _context.Flats.FirstOrDefaultAsync(f =>
                f.FlatId == flatId &&
                f.SocietyId == societyId);

            if (flat == null)
                throw new KeyNotFoundException(
                    "Flat not found in this society.");

            await GetActiveFloorAsync(societyId, flat.FloorId);

            var flatNumber = request.FlatNumber.Trim();

            var duplicate = await _context.Flats.AnyAsync(f =>
                f.FloorId == flat.FloorId &&
                f.FlatId != flatId &&
                f.FlatNumber.ToLower() == flatNumber.ToLower());

            if (duplicate)
                throw new InvalidOperationException(
                    "A flat with this number already exists on this floor.");

            flat.FlatNumber = flatNumber;
            flat.IsOccupied = request.IsOccupied;

            await _context.SaveChangesAsync();

            return MapResponse(flat);
        }

        public async Task<FlatResponse> SetStatusAsync(
            int societyId, int flatId, UpdateFlatStatusRequest request)
        {
            var plan = await ValidateSocietyAccessAsync(societyId);

            var flat = await _context.Flats.FirstOrDefaultAsync(f =>
                f.FlatId == flatId &&
                f.SocietyId == societyId);

            if (flat == null)
                throw new KeyNotFoundException(
                    "Flat not found in this society.");

            if (request.IsActive && !flat.IsActive)
            {
                await GetActiveFloorAsync(societyId, flat.FloorId);

                var activeFlatCount = await _context.Flats.CountAsync(f =>
                    f.SocietyId == societyId && f.IsActive);

                if (activeFlatCount >= plan.MaxFlats)
                    throw new InvalidOperationException(
                        "The subscription plan's flat limit has been reached.");
            }

            flat.IsActive = request.IsActive;

            await _context.SaveChangesAsync();

            return MapResponse(flat);
        }

        private static FlatResponse MapResponse(Flat flat)
        {
            return new FlatResponse
            {
                FlatId = flat.FlatId,
                SocietyId = flat.SocietyId,
                WingId = flat.WingId,
                FloorId = flat.FloorId,
                FlatNumber = flat.FlatNumber,
                IsOccupied = flat.IsOccupied,
                IsActive = flat.IsActive,
                CreatedAt = flat.CreatedAt
            };
        }
    }
}
