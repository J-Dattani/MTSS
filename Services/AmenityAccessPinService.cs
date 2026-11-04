
using Microsoft.EntityFrameworkCore;
using MTSS.Data;
using MTSS.DTOs.AmenityAccessPin;
using MTSS.Models;
using System.Security.Cryptography;
using System.Text;

namespace MTSS.Services
{
    public class AmenityAccessPinService
    {
        private readonly ApplicationDbContext _context;
        private readonly string _hashKey;

        private const int PinExpiryMinutes = 15;

        public AmenityAccessPinService(
            ApplicationDbContext context,
            IConfiguration configuration)
        {
            _context = context;

            _hashKey = configuration["AmenityPin:HashKey"]
                ?? throw new InvalidOperationException(
                    "Amenity PIN hashing key is not configured.");

            if (Convert.FromBase64String(_hashKey).Length < 32)
                throw new InvalidOperationException(
                    "Amenity PIN hashing key must contain at least 32 bytes.");
        }

        // Validate society and subscription feature
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

            if (!subscription.SubscriptionPlan.AmenityPinEnabled)
                throw new InvalidOperationException(
                    "Amenity PIN management is not enabled for this subscription plan.");
        }

        // Validate authenticated resident and retrieve profile
        private async Task<ResidentProfile> GetActiveResidentAsync(
            string applicationUserId)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == applicationUserId);

            if (user == null || !user.IsActive || user.SocietyId == null)
                throw new UnauthorizedAccessException(
                    "Resident account is inactive or invalid.");

            var resident = await _context.ResidentProfiles
                .FirstOrDefaultAsync(r =>
                    r.ApplicationUserId == user.Id &&
                    r.SocietyId == user.SocietyId &&
                    r.IsActive);

            if (resident == null)
                throw new UnauthorizedAccessException(
                    "No active resident profile is linked to this account.");

            var flat = await _context.Flats.FirstOrDefaultAsync(f =>
                f.FlatId == resident.FlatId &&
                f.SocietyId == user.SocietyId &&
                f.IsActive);

            if (flat == null)
                throw new InvalidOperationException(
                    "Resident's flat is inactive or unavailable.");

            var activeFloorAndWing = await _context.Floors.AnyAsync(f =>
                f.FloorId == flat.FloorId &&
                f.SocietyId == user.SocietyId &&
                f.IsActive &&
                _context.Wings.Any(w =>
                    w.WingId == f.WingId &&
                    w.SocietyId == user.SocietyId &&
                    w.IsActive));

            if (!activeFloorAndWing)
                throw new InvalidOperationException(
                    "Resident's floor or wing is inactive or unavailable.");

            return resident;
        }

        // Generate secure six-digit PIN
        public async Task<AmenityPinResponse> GenerateAsync(
            string applicationUserId,
            GenerateAmenityPinRequest request)
        {
            var resident = await GetActiveResidentAsync(applicationUserId);

            await ValidateSocietyAccessAsync(resident.SocietyId);

            var amenity = await _context.Amenities.FirstOrDefaultAsync(a =>
                a.AmenityId == request.AmenityId &&
                a.SocietyId == resident.SocietyId &&
                a.IsActive);

            if (amenity == null)
                throw new KeyNotFoundException(
                    "Active amenity not found in this society.");

            var pin = RandomNumberGenerator
                .GetInt32(0, 1000000)
                .ToString("D6");

            var now = DateTime.UtcNow;

            var pinRecord = new AmenityAccessPin
            {
                SocietyId = resident.SocietyId,
                AmenityId = amenity.AmenityId,
                FlatId = resident.FlatId,
                ResidentProfileId = resident.ResidentProfileId,
                PinHash = HashPin(pin),
                GeneratedAt = now,
                ExpiresAt = now.AddMinutes(PinExpiryMinutes),
                UsedAt = null,
                Status = "Active"
            };

            _context.AmenityAccessPins.Add(pinRecord);
            await _context.SaveChangesAsync();

            return new AmenityPinResponse
            {
                AmenityAccessPinId = pinRecord.AmenityAccessPinId,
                AmenityId = amenity.AmenityId,
                AmenityName = amenity.Name,
                Pin = pin,
                GeneratedAt = pinRecord.GeneratedAt,
                ExpiresAt = pinRecord.ExpiresAt,
                UsedAt = null,
                Status = pinRecord.Status
            };
        }

        // Resident: Get own PIN records
        public async Task<List<AmenityPinResponse>> GetResidentPinsAsync(
            string applicationUserId)
        {
            var resident = await GetActiveResidentAsync(applicationUserId);

            await ValidateSocietyAccessAsync(resident.SocietyId);

            return await (
                from pin in _context.AmenityAccessPins.AsNoTracking()
                join amenity in _context.Amenities
                    on pin.AmenityId equals amenity.AmenityId
                where pin.ResidentProfileId == resident.ResidentProfileId &&
                      pin.SocietyId == resident.SocietyId
                orderby pin.GeneratedAt descending
                select new AmenityPinResponse
                {
                    AmenityAccessPinId = pin.AmenityAccessPinId,
                    AmenityId = pin.AmenityId,
                    AmenityName = amenity.Name,
                    Pin = null,
                    GeneratedAt = pin.GeneratedAt,
                    ExpiresAt = pin.ExpiresAt,
                    UsedAt = pin.UsedAt,
                    Status = pin.Status
                }
            ).ToListAsync();
        }

        // Resident: Get own PIN by ID
        public async Task<AmenityPinResponse> GetResidentPinByIdAsync(
            string applicationUserId,
            int pinId)
        {
            var resident = await GetActiveResidentAsync(applicationUserId);

            await ValidateSocietyAccessAsync(resident.SocietyId);

            var result = await (
                from pin in _context.AmenityAccessPins.AsNoTracking()
                join amenity in _context.Amenities
                    on pin.AmenityId equals amenity.AmenityId
                where pin.AmenityAccessPinId == pinId &&
                      pin.ResidentProfileId == resident.ResidentProfileId &&
                      pin.SocietyId == resident.SocietyId
                select new AmenityPinResponse
                {
                    AmenityAccessPinId = pin.AmenityAccessPinId,
                    AmenityId = pin.AmenityId,
                    AmenityName = amenity.Name,
                    Pin = null,
                    GeneratedAt = pin.GeneratedAt,
                    ExpiresAt = pin.ExpiresAt,
                    UsedAt = pin.UsedAt,
                    Status = pin.Status
                }
            ).FirstOrDefaultAsync();

            if (result == null)
                throw new KeyNotFoundException("Amenity PIN not found.");

            return result;
        }

        // Resident: Revoke unused PIN
        public async Task<AmenityPinResponse> RevokeAsync(
            string applicationUserId,
            int pinId)
        {
            var resident = await GetActiveResidentAsync(applicationUserId);

            await ValidateSocietyAccessAsync(resident.SocietyId);

            var pin = await _context.AmenityAccessPins.FirstOrDefaultAsync(p =>
                p.AmenityAccessPinId == pinId &&
                p.ResidentProfileId == resident.ResidentProfileId &&
                p.SocietyId == resident.SocietyId);

            if (pin == null)
                throw new KeyNotFoundException("Amenity PIN not found.");

            if (pin.Status != "Active" || pin.UsedAt != null)
                throw new InvalidOperationException(
                    "Only active, unused PINs can be revoked.");

            pin.Status = "Revoked";

            await _context.SaveChangesAsync();

            var amenity = await _context.Amenities
                .FirstOrDefaultAsync(a => a.AmenityId == pin.AmenityId);

            return MapResponse(pin, amenity?.Name ?? string.Empty);
        }

        // SocietyAdmin: Get all PIN records in own society
        public async Task<List<AmenityPinResponse>> GetAllAsync(
            int societyId)
        {
            await ValidateSocietyAccessAsync(societyId);

            return await (
                from pin in _context.AmenityAccessPins.AsNoTracking()
                join amenity in _context.Amenities
                    on pin.AmenityId equals amenity.AmenityId
                where pin.SocietyId == societyId
                orderby pin.GeneratedAt descending
                select new AmenityPinResponse
                {
                    AmenityAccessPinId = pin.AmenityAccessPinId,
                    AmenityId = pin.AmenityId,
                    AmenityName = amenity.Name,
                    Pin = null,
                    GeneratedAt = pin.GeneratedAt,
                    ExpiresAt = pin.ExpiresAt,
                    UsedAt = pin.UsedAt,
                    Status = pin.Status
                }
            ).ToListAsync();
        }

        // SocietyAdmin: Get all amenity access logs in own society
        public async Task<List<AmenityAccessLogResponse>> GetAccessLogsAsync(
            int societyId)
        {
            await ValidateSocietyAccessAsync(societyId);

            return await (
                from log in _context.AmenityAccessLogs.AsNoTracking()
                join amenity in _context.Amenities
                    on log.AmenityId equals amenity.AmenityId
                where log.SocietyId == societyId
                orderby log.AttemptedAt descending
                select new AmenityAccessLogResponse
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

        // SocietyAdmin: Validate PIN at amenity entrance
        public async Task<AmenityPinValidationResponse> ValidatePinAsync(
            int societyId,
            ValidateAmenityPinRequest request)
        {
            await ValidateSocietyAccessAsync(societyId);

            var amenity = await _context.Amenities.FirstOrDefaultAsync(a =>
                a.AmenityId == request.AmenityId &&
                a.SocietyId == societyId &&
                a.IsActive);

            if (amenity == null)
                throw new KeyNotFoundException(
                    "Active amenity not found in this society.");

            var attemptedAt = DateTime.UtcNow;
            var submittedHash = HashPin(request.Pin);

            // Retrieve candidate records and compare hashes in constant time.
            var candidates = await _context.AmenityAccessPins
                .Where(p =>
                    p.SocietyId == societyId &&
                    p.AmenityId == request.AmenityId)
                .ToListAsync();

            var submittedBytes = Convert.FromBase64String(submittedHash);

            var matchingPin = candidates.FirstOrDefault(p =>
            {
                var storedBytes = Convert.FromBase64String(p.PinHash);

                return storedBytes.Length == submittedBytes.Length &&
                    CryptographicOperations.FixedTimeEquals(
                        storedBytes, submittedBytes);
            });

            // An unmatched PIN cannot be associated with the required
            // resident/flat foreign keys in the existing access-log model.
            if (matchingPin == null)
            {
                return new AmenityPinValidationResponse
                {
                    IsSuccessful = false,
                    Message = "Invalid PIN.",
                    AmenityAccessLogId = null,
                    AttemptedAt = attemptedAt
                };
            }

            string? failureReason = null;

            if (matchingPin.Status != "Active")
                failureReason = "PIN is not active.";
            else if (matchingPin.UsedAt != null)
                failureReason = "PIN has already been used.";
            else if (matchingPin.ExpiresAt <= attemptedAt)
            {
                matchingPin.Status = "Expired";
                failureReason = "PIN has expired.";
            }

            // Amenity operating hours are interpreted using server local time.
            var localTime = DateTime.Now.TimeOfDay;

            if (failureReason == null)
            {
                var isWithinOpeningHours =
                    amenity.OpeningTime <= amenity.ClosingTime
                    ? localTime >= amenity.OpeningTime &&
                      localTime <= amenity.ClosingTime
                    : localTime >= amenity.OpeningTime ||
                      localTime <= amenity.ClosingTime;

                if (!isWithinOpeningHours)
                    failureReason = "Amenity is currently closed.";
            }

            // Recheck resident, flat and amenity availability.
            if (failureReason == null)
            {
                var residentIsActive = await _context.ResidentProfiles
                    .AnyAsync(r =>
                        r.ResidentProfileId == matchingPin.ResidentProfileId &&
                        r.SocietyId == societyId &&
                        r.FlatId == matchingPin.FlatId &&
                        r.IsActive &&
                        _context.Flats.Any(f =>
                            f.FlatId == matchingPin.FlatId &&
                            f.SocietyId == societyId &&
                            f.IsActive &&
                            _context.Floors.Any(fl =>
                                fl.FloorId == f.FloorId &&
                                fl.SocietyId == societyId &&
                                fl.IsActive &&
                                _context.Wings.Any(w =>
                                    w.WingId == fl.WingId &&
                                    w.SocietyId == societyId &&
                                    w.IsActive))));

                if (!residentIsActive)
                    failureReason = "Resident or flat is inactive or unavailable.";
            }

            if (failureReason == null)
            {
                matchingPin.Status = "Used";
                matchingPin.UsedAt = attemptedAt;
            }

            var accessLog = new AmenityAccessLog
            {
                SocietyId = societyId,
                AmenityId = amenity.AmenityId,
                FlatId = matchingPin.FlatId,
                ResidentProfileId = matchingPin.ResidentProfileId,
                IsSuccessful = failureReason == null,
                AttemptedAt = attemptedAt,
                FailureReason = failureReason
            };

            _context.AmenityAccessLogs.Add(accessLog);

            await _context.SaveChangesAsync();

            return new AmenityPinValidationResponse
            {
                IsSuccessful = failureReason == null,
                Message = failureReason ?? "PIN validated successfully. Entry allowed.",
                AmenityAccessLogId = accessLog.AmenityAccessLogId,
                AttemptedAt = attemptedAt
            };
        }

        // Hash PIN using server-side HMAC key
        private string HashPin(string pin)
        {
            var key = Convert.FromBase64String(_hashKey);

            using var hmac = new HMACSHA256(key);

            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(pin));

            return Convert.ToBase64String(hash);
        }

        private static AmenityPinResponse MapResponse(
            AmenityAccessPin pin,
            string amenityName)
        {
            return new AmenityPinResponse
            {
                AmenityAccessPinId = pin.AmenityAccessPinId,
                AmenityId = pin.AmenityId,
                AmenityName = amenityName,
                Pin = null,
                GeneratedAt = pin.GeneratedAt,
                ExpiresAt = pin.ExpiresAt,
                UsedAt = pin.UsedAt,
                Status = pin.Status
            };
        }
    }
}
