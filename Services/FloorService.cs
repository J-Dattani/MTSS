
using Microsoft.EntityFrameworkCore;
using MTSS.Data;
using MTSS.DTOs.Floor;
using MTSS.Models;

namespace MTSS.Services
{
    public class FloorService
    {
        private readonly ApplicationDbContext _context;

        public FloorService(ApplicationDbContext context)
        {
            _context = context;
        }

        private async Task ValidateSocietyAccessAsync(int societyId)
        {
            var society = await _context.Societies
                .AnyAsync(s => s.SocietyId == societyId && s.IsActive);

            if (!society)
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

            if (subscription == null)
                throw new InvalidOperationException(
                    "Society does not have a valid active subscription.");
        }

        private async Task<Wing> GetActiveWingAsync(
            int societyId, int wingId)
        {
            var wing = await _context.Wings.FirstOrDefaultAsync(w =>
                w.WingId == wingId &&
                w.SocietyId == societyId &&
                w.IsActive);

            if (wing == null)
                throw new KeyNotFoundException(
                    "Active wing not found in this society.");

            return wing;
        }

        public async Task<List<FloorResponse>> GetAllAsync(int societyId)
        {
            await ValidateSocietyAccessAsync(societyId);

            return await _context.Floors
                .Where(f => f.SocietyId == societyId)
                .OrderBy(f => f.WingId)
                .ThenBy(f => f.FloorNumber)
                .Select(f => new FloorResponse
                {
                    FloorId = f.FloorId,
                    SocietyId = f.SocietyId,
                    WingId = f.WingId,
                    Name = f.Name,
                    FloorNumber = f.FloorNumber,
                    IsActive = f.IsActive,
                    CreatedAt = f.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<FloorResponse> GetByIdAsync(
            int societyId, int floorId)
        {
            await ValidateSocietyAccessAsync(societyId);

            var floor = await _context.Floors.FirstOrDefaultAsync(f =>
                f.FloorId == floorId &&
                f.SocietyId == societyId);

            if (floor == null)
                throw new KeyNotFoundException(
                    "Floor not found in this society.");

            return MapResponse(floor);
        }

        public async Task<FloorResponse> CreateAsync(
            int societyId, CreateFloorRequest request)
        {
            await ValidateSocietyAccessAsync(societyId);

            var wing = await GetActiveWingAsync(
                societyId, request.WingId);

            var name = request.Name.Trim();

            var duplicate = await _context.Floors.AnyAsync(f =>
                f.WingId == wing.WingId &&
                (f.Name.ToLower() == name.ToLower() ||
                 f.FloorNumber == request.FloorNumber));

            if (duplicate)
                throw new InvalidOperationException(
                    "A floor with this name or floor number already exists in this wing.");

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

            if (subscription == null)
                throw new InvalidOperationException(
                    "Society does not have a valid active subscription.");

            var plan = subscription.SubscriptionPlan;

            if (plan == null)
                throw new InvalidOperationException(
                    "The subscription plan could not be found.");

            var activeFloorCount = await _context.Floors.CountAsync(f =>
                f.WingId == wing.WingId &&
                f.IsActive);

            if (activeFloorCount >= plan.MaxFloorsPerWing)
                throw new InvalidOperationException(
                    "The subscription plan's floor limit for this wing has been reached.");

            var floor = new Floor
            {
                SocietyId = societyId,
                WingId = wing.WingId,
                Name = name,
                FloorNumber = request.FloorNumber,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Floors.Add(floor);
            await _context.SaveChangesAsync();

            return MapResponse(floor);
        }

        public async Task<FloorResponse> UpdateAsync(
            int societyId, int floorId, UpdateFloorRequest request)
        {
            await ValidateSocietyAccessAsync(societyId);

            var floor = await _context.Floors.FirstOrDefaultAsync(f =>
                f.FloorId == floorId &&
                f.SocietyId == societyId);

            if (floor == null)
                throw new KeyNotFoundException(
                    "Floor not found in this society.");

            await GetActiveWingAsync(societyId, floor.WingId);

            var name = request.Name.Trim();

            var duplicate = await _context.Floors.AnyAsync(f =>
                f.WingId == floor.WingId &&
                f.FloorId != floorId &&
                (f.Name.ToLower() == name.ToLower() ||
                 f.FloorNumber == request.FloorNumber));

            if (duplicate)
                throw new InvalidOperationException(
                    "A floor with this name or floor number already exists in this wing.");

            floor.Name = name;
            floor.FloorNumber = request.FloorNumber;

            await _context.SaveChangesAsync();

            return MapResponse(floor);
        }

        public async Task<FloorResponse> SetStatusAsync(
            int societyId, int floorId,
            UpdateFloorStatusRequest request)
        {
            await ValidateSocietyAccessAsync(societyId);

            var floor = await _context.Floors.FirstOrDefaultAsync(f =>
                f.FloorId == floorId &&
                f.SocietyId == societyId);

            if (floor == null)
                throw new KeyNotFoundException(
                    "Floor not found in this society.");

            if (request.IsActive)
            {
                await GetActiveWingAsync(societyId, floor.WingId);

                if (!floor.IsActive)
                {
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

                    if (subscription == null)
                        throw new InvalidOperationException(
                            "Society does not have a valid active subscription.");

                    var plan = subscription.SubscriptionPlan;

                    if (plan == null)
                        throw new InvalidOperationException(
                            "The subscription plan could not be found.");

                    var activeFloorCount = await _context.Floors.CountAsync(f =>
                        f.WingId == floor.WingId &&
                        f.IsActive);

                    if (activeFloorCount >= plan.MaxFloorsPerWing)
                        throw new InvalidOperationException(
                            "The subscription plan's floor limit for this wing has been reached.");
                }
            }

            floor.IsActive = request.IsActive;

            await _context.SaveChangesAsync();

            return MapResponse(floor);
        }

        private static FloorResponse MapResponse(Floor floor)
        {
            return new FloorResponse
            {
                FloorId = floor.FloorId,
                SocietyId = floor.SocietyId,
                WingId = floor.WingId,
                Name = floor.Name,
                FloorNumber = floor.FloorNumber,
                IsActive = floor.IsActive,
                CreatedAt = floor.CreatedAt
            };
        }
    }
}
