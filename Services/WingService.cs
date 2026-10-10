
using Microsoft.EntityFrameworkCore;
using MTSS.Data;
using MTSS.DTOs.Wing;
using MTSS.Models;

namespace MTSS.Services
{
    public class WingService
    {
        private readonly ApplicationDbContext _context;

        public WingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<WingResponse>> GetAllAsync(int societyId)
        {
            await ValidateSocietyAccessAsync(societyId);

            var wings = await _context.Wings
                .AsNoTracking()
                .Where(w => w.SocietyId == societyId)
                .OrderBy(w => w.WingId)
                .ToListAsync();

            return wings.Select(MapToResponse).ToList();
        }

        public async Task<WingResponse> GetByIdAsync(
            int societyId,
            int wingId)
        {
            await ValidateSocietyAccessAsync(societyId);

            var wing = await _context.Wings
                .AsNoTracking()
                .FirstOrDefaultAsync(w =>
                    w.WingId == wingId &&
                    w.SocietyId == societyId);

            if (wing == null)
                throw new KeyNotFoundException("Wing not found.");

            return MapToResponse(wing);
        }

        public async Task<WingResponse> CreateAsync(
            int societyId,
            CreateWingRequest request)
        {
            var plan = await ValidateSocietyAccessAsync(societyId);
            var name = request.Name?.Trim();

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Wing name is required.");

            var duplicate = await _context.Wings.AnyAsync(w =>
                w.SocietyId == societyId &&
                w.Name.ToLower() == name.ToLower());

            if (duplicate)
                throw new InvalidOperationException(
                    "A wing with this name already exists in the society.");

            var activeWingCount = await _context.Wings.CountAsync(w =>
                w.SocietyId == societyId && w.IsActive);

            if (activeWingCount >= plan.MaxWings)
                throw new InvalidOperationException(
                    "Wing quota has been reached for the current subscription plan.");

            var wing = new Wing
            {
                SocietyId = societyId,
                Name = name,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Wings.Add(wing);
            await _context.SaveChangesAsync();

            return MapToResponse(wing);
        }

        public async Task<WingResponse> UpdateAsync(
            int societyId,
            int wingId,
            UpdateWingRequest request)
        {
            await ValidateSocietyAccessAsync(societyId);

            var wing = await _context.Wings.FirstOrDefaultAsync(w =>
                w.WingId == wingId &&
                w.SocietyId == societyId);

            if (wing == null)
                throw new KeyNotFoundException("Wing not found.");

            var name = request.Name?.Trim();

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Wing name is required.");

            var duplicate = await _context.Wings.AnyAsync(w =>
                w.SocietyId == societyId &&
                w.WingId != wingId &&
                w.Name.ToLower() == name.ToLower());

            if (duplicate)
                throw new InvalidOperationException(
                    "Another wing with this name already exists in the society.");

            wing.Name = name;

            await _context.SaveChangesAsync();

            return MapToResponse(wing);
        }

        public async Task<WingResponse> SetStatusAsync(
            int societyId,
            int wingId,
            UpdateWingStatusRequest request)
        {
            var plan = await ValidateSocietyAccessAsync(societyId);

            var wing = await _context.Wings.FirstOrDefaultAsync(w =>
                w.WingId == wingId &&
                w.SocietyId == societyId);

            if (wing == null)
                throw new KeyNotFoundException("Wing not found.");

            if (request.IsActive && !wing.IsActive)
            {
                var activeWingCount = await _context.Wings.CountAsync(w =>
                    w.SocietyId == societyId && w.IsActive);

                if (activeWingCount >= plan.MaxWings)
                    throw new InvalidOperationException(
                        "Wing quota has been reached for the current subscription plan.");
            }

            wing.IsActive = request.IsActive;

            await _context.SaveChangesAsync();

            return MapToResponse(wing);
        }

        private async Task<SubscriptionPlan>
            ValidateSocietyAccessAsync(int societyId)
        {
            var society = await _context.Societies
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.SocietyId == societyId);

            if (society == null)
                throw new KeyNotFoundException("Society not found.");

            if (!society.IsActive)
                throw new UnauthorizedAccessException(
                    "This society is inactive.");

            var now = DateTime.UtcNow;

            var subscription = await _context.SocietySubscriptions
                .Include(s => s.SubscriptionPlan)
                .Where(s =>
                    s.SocietyId == societyId &&
                    s.IsActive &&
                    s.StartDate <= now &&
                    s.EndDate > now &&
                    (s.Status == "Trial" || s.Status == "Active"))
                .OrderByDescending(s => s.SocietySubscriptionId)
                .FirstOrDefaultAsync();

            if (subscription == null)
                throw new UnauthorizedAccessException(
                    "The society does not have a currently valid subscription.");

            if (subscription.SubscriptionPlan == null ||
                !subscription.SubscriptionPlan.IsActive)
                throw new UnauthorizedAccessException(
                    "The society's subscription plan is inactive or unavailable.");

            return subscription.SubscriptionPlan;
        }

        private static WingResponse MapToResponse(Wing wing)
        {
            return new WingResponse
            {
                WingId = wing.WingId,
                SocietyId = wing.SocietyId,
                Name = wing.Name,
                IsActive = wing.IsActive,
                CreatedAt = wing.CreatedAt
            };
        }
    }
}
