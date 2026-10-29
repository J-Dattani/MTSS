
using Microsoft.EntityFrameworkCore;
using MTSS.Data;
using MTSS.DTOs.Amenity;
using MTSS.Models;

namespace MTSS.Services
{
    public class AmenityService
    {
        private readonly ApplicationDbContext _context;

        public AmenityService(ApplicationDbContext context)
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

            if (subscription?.SubscriptionPlan == null)
                throw new InvalidOperationException(
                    "Society does not have a valid active subscription.");

            return subscription.SubscriptionPlan;
        }

        public async Task<List<AmenityResponse>> GetAllAsync(int societyId)
        {
            await ValidateSocietyAccessAsync(societyId);

            return await _context.Amenities
                .Where(a => a.SocietyId == societyId)
                .OrderBy(a => a.Name)
                .Select(a => new AmenityResponse
                {
                    AmenityId = a.AmenityId,
                    SocietyId = a.SocietyId,
                    Name = a.Name,
                    Description = a.Description,
                    OpeningTime = a.OpeningTime,
                    ClosingTime = a.ClosingTime,
                    IsActive = a.IsActive,
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<AmenityResponse> GetByIdAsync(
            int societyId, int amenityId)
        {
            await ValidateSocietyAccessAsync(societyId);

            var amenity = await _context.Amenities.FirstOrDefaultAsync(a =>
                a.AmenityId == amenityId &&
                a.SocietyId == societyId);

            if (amenity == null)
                throw new KeyNotFoundException(
                    "Amenity not found in this society.");

            return MapResponse(amenity);
        }

        public async Task<AmenityResponse> CreateAsync(
            int societyId, CreateAmenityRequest request)
        {
            var plan = await ValidateSocietyAccessAsync(societyId);

            var name = request.Name.Trim();
            var description = request.Description?.Trim() ?? string.Empty;

            ValidateDetails(name, request.OpeningTime, request.ClosingTime);

            var duplicate = await _context.Amenities.AnyAsync(a =>
                a.SocietyId == societyId &&
                a.Name.ToLower() == name.ToLower());

            if (duplicate)
                throw new InvalidOperationException(
                    "An amenity with this name already exists in this society.");

            var activeAmenityCount = await _context.Amenities.CountAsync(a =>
                a.SocietyId == societyId && a.IsActive);

            if (activeAmenityCount >= plan.MaxAmenities)
                throw new InvalidOperationException(
                    "The subscription plan's amenity limit has been reached.");

            var amenity = new Amenity
            {
                SocietyId = societyId,
                Name = name,
                Description = description,
                OpeningTime = request.OpeningTime,
                ClosingTime = request.ClosingTime,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Amenities.Add(amenity);
            await _context.SaveChangesAsync();

            return MapResponse(amenity);
        }

        public async Task<AmenityResponse> UpdateAsync(
            int societyId, int amenityId, UpdateAmenityRequest request)
        {
            await ValidateSocietyAccessAsync(societyId);

            var amenity = await _context.Amenities.FirstOrDefaultAsync(a =>
                a.AmenityId == amenityId &&
                a.SocietyId == societyId);

            if (amenity == null)
                throw new KeyNotFoundException(
                    "Amenity not found in this society.");

            var name = request.Name.Trim();
            var description = request.Description?.Trim() ?? string.Empty;

            ValidateDetails(name, request.OpeningTime, request.ClosingTime);

            var duplicate = await _context.Amenities.AnyAsync(a =>
                a.SocietyId == societyId &&
                a.AmenityId != amenityId &&
                a.Name.ToLower() == name.ToLower());

            if (duplicate)
                throw new InvalidOperationException(
                    "An amenity with this name already exists in this society.");

            amenity.Name = name;
            amenity.Description = description;
            amenity.OpeningTime = request.OpeningTime;
            amenity.ClosingTime = request.ClosingTime;

            await _context.SaveChangesAsync();

            return MapResponse(amenity);
        }

        public async Task<AmenityResponse> SetStatusAsync(
            int societyId,
            int amenityId,
            UpdateAmenityStatusRequest request)
        {
            var plan = await ValidateSocietyAccessAsync(societyId);

            var amenity = await _context.Amenities.FirstOrDefaultAsync(a =>
                a.AmenityId == amenityId &&
                a.SocietyId == societyId);

            if (amenity == null)
                throw new KeyNotFoundException(
                    "Amenity not found in this society.");

            if (request.IsActive && !amenity.IsActive)
            {
                var activeAmenityCount = await _context.Amenities.CountAsync(a =>
                    a.SocietyId == societyId && a.IsActive);

                if (activeAmenityCount >= plan.MaxAmenities)
                    throw new InvalidOperationException(
                        "The subscription plan's amenity limit has been reached.");
            }

            amenity.IsActive = request.IsActive;

            await _context.SaveChangesAsync();

            return MapResponse(amenity);
        }

        private static void ValidateDetails(
            string name, TimeSpan openingTime, TimeSpan closingTime)
        {
            if (string.IsNullOrWhiteSpace(name) ||
                name.Length < 2 || name.Length > 100)
            {
                throw new ArgumentException(
                    "Amenity name must be between 2 and 100 characters.");
            }

            if (openingTime < TimeSpan.Zero ||
                openingTime >= TimeSpan.FromDays(1) ||
                closingTime < TimeSpan.Zero ||
                closingTime >= TimeSpan.FromDays(1))
            {
                throw new ArgumentException(
                    "Opening and closing times must be valid times of day.");
            }

            if (openingTime >= closingTime)
                throw new ArgumentException(
                    "Opening time must be earlier than closing time.");
        }

        private static AmenityResponse MapResponse(Amenity amenity)
        {
            return new AmenityResponse
            {
                AmenityId = amenity.AmenityId,
                SocietyId = amenity.SocietyId,
                Name = amenity.Name,
                Description = amenity.Description,
                OpeningTime = amenity.OpeningTime,
                ClosingTime = amenity.ClosingTime,
                IsActive = amenity.IsActive,
                CreatedAt = amenity.CreatedAt
            };
        }
    }
}
