
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using MTSS.Data;
using MTSS.DTOs.Resident;
using MTSS.Models;

namespace MTSS.Services
{
    public class ResidentService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ResidentService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
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

        private async Task<Flat> GetActiveFlatAsync(
            int societyId, int flatId)
        {
            var flat = await _context.Flats.FirstOrDefaultAsync(f =>
                f.FlatId == flatId &&
                f.SocietyId == societyId &&
                f.IsActive);

            if (flat == null)
                throw new KeyNotFoundException(
                    "Active flat not found in this society.");

            var floorAndWingAreActive =
                await _context.Floors.AnyAsync(f =>
                    f.FloorId == flat.FloorId &&
                    f.SocietyId == societyId &&
                    f.IsActive &&
                    _context.Wings.Any(w =>
                        w.WingId == f.WingId &&
                        w.SocietyId == societyId &&
                        w.IsActive));

            if (!floorAndWingAreActive)
                throw new InvalidOperationException(
                    "The flat's floor or wing is inactive or unavailable.");

            return flat;
        }

        public async Task<List<ResidentResponse>> GetAllAsync(int societyId)
        {
            await ValidateSocietyAccessAsync(societyId);

            return await _context.ResidentProfiles
                .Where(r => r.SocietyId == societyId)
                .OrderBy(r => r.FullName)
                .Select(r => new ResidentResponse
                {
                    ResidentProfileId = r.ResidentProfileId,
                    SocietyId = r.SocietyId,
                    FlatId = r.FlatId,
                    FullName = r.FullName,
                    MobileNumber = r.MobileNumber,
                    IsHome = r.IsHome,
                    IsActive = r.IsActive,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<ResidentResponse> GetByIdAsync(
            int societyId, int residentProfileId)
        {
            await ValidateSocietyAccessAsync(societyId);

            var resident = await _context.ResidentProfiles
                .FirstOrDefaultAsync(r =>
                    r.ResidentProfileId == residentProfileId &&
                    r.SocietyId == societyId);

            if (resident == null)
                throw new KeyNotFoundException(
                    "Resident profile not found in this society.");

            return MapResponse(resident);
        }

        public async Task<ResidentResponse> CreateAsync(
            int societyId, CreateResidentRequest request)
        {
            await ValidateSocietyAccessAsync(societyId);

            var flat = await GetActiveFlatAsync(societyId, request.FlatId);

            var fullName = request.FullName.Trim();
            var mobileNumber = request.MobileNumber.Trim();

            var duplicate = await _context.ResidentProfiles.AnyAsync(r =>
                r.SocietyId == societyId &&
                r.FlatId == flat.FlatId &&
                r.MobileNumber == mobileNumber &&
                r.IsActive);

            if (duplicate)
                throw new InvalidOperationException(
                    "An active resident profile with this mobile number already exists for this flat.");

            var resident = new ResidentProfile
            {
                SocietyId = societyId,
                FlatId = flat.FlatId,
                FullName = fullName,
                MobileNumber = mobileNumber,
                IsHome = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.ResidentProfiles.Add(resident);
            await _context.SaveChangesAsync();

            return MapResponse(resident);
        }

        public async Task<ResidentResponse> UpdateAsync(
            int societyId,
            int residentProfileId,
            UpdateResidentRequest request)
        {
            await ValidateSocietyAccessAsync(societyId);

            var resident = await _context.ResidentProfiles
                .FirstOrDefaultAsync(r =>
                    r.ResidentProfileId == residentProfileId &&
                    r.SocietyId == societyId);

            if (resident == null)
                throw new KeyNotFoundException(
                    "Resident profile not found in this society.");

            await GetActiveFlatAsync(societyId, resident.FlatId);

            var fullName = request.FullName.Trim();
            var mobileNumber = request.MobileNumber.Trim();

            var duplicate = await _context.ResidentProfiles.AnyAsync(r =>
                r.SocietyId == societyId &&
                r.FlatId == resident.FlatId &&
                r.ResidentProfileId != residentProfileId &&
                r.MobileNumber == mobileNumber &&
                r.IsActive);

            if (duplicate)
                throw new InvalidOperationException(
                    "An active resident profile with this mobile number already exists for this flat.");

            resident.FullName = fullName;
            resident.MobileNumber = mobileNumber;
            resident.IsHome = request.IsHome;

            await _context.SaveChangesAsync();

            return MapResponse(resident);
        }

        public async Task<ResidentResponse> SetStatusAsync(
            int societyId,
            int residentProfileId,
            UpdateResidentStatusRequest request)
        {
            await ValidateSocietyAccessAsync(societyId);

            var resident = await _context.ResidentProfiles
                .FirstOrDefaultAsync(r =>
                    r.ResidentProfileId == residentProfileId &&
                    r.SocietyId == societyId);

            if (resident == null)
                throw new KeyNotFoundException(
                    "Resident profile not found in this society.");

            if (request.IsActive)
                await GetActiveFlatAsync(societyId, resident.FlatId);

            resident.IsActive = request.IsActive;

            await _context.SaveChangesAsync();

            return MapResponse(resident);
        }

        // Create a Resident Identity account and link it to the profile
        public async Task<ResidentResponse> CreateAccountAsync(
            int societyId,
            int residentProfileId,
            CreateResidentAccountRequest request)
        {
            await ValidateSocietyAccessAsync(societyId);

            var resident = await _context.ResidentProfiles
                .FirstOrDefaultAsync(r =>
                    r.ResidentProfileId == residentProfileId &&
                    r.SocietyId == societyId);

            if (resident == null)
                throw new KeyNotFoundException(
                    "Resident profile not found in this society.");

            if (!resident.IsActive)
                throw new InvalidOperationException(
                    "Inactive resident profiles cannot have accounts.");

            await GetActiveFlatAsync(societyId, resident.FlatId);

            if (!string.IsNullOrEmpty(resident.ApplicationUserId))
                throw new InvalidOperationException(
                    "This resident profile is already linked to an account.");

            var email = request.Email.Trim();

            var existingUser = await _userManager.FindByEmailAsync(email);

            if (existingUser != null)
                throw new InvalidOperationException(
                    "An account with this email already exists.");

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = resident.FullName,
                SocietyId = societyId,
                IsActive = true,
                EmailConfirmed = false
            };

            var createResult = await _userManager.CreateAsync(
                user,
                request.Password);

            if (!createResult.Succeeded)
            {
                var errors = string.Join(
                    "; ",
                    createResult.Errors.Select(e => e.Description));

                throw new ArgumentException(
                    $"Resident account creation failed: {errors}");
            }

            var roleResult = await _userManager.AddToRoleAsync(
                user,
                "Resident");

            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);

                var errors = string.Join(
                    "; ",
                    roleResult.Errors.Select(e => e.Description));

                throw new InvalidOperationException(
                    $"Resident role assignment failed: {errors}");
            }

            resident.ApplicationUserId = user.Id;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch
            {
                await _userManager.DeleteAsync(user);
                throw;
            }

            return MapResponse(resident);
        }

        // Link an existing Identity account to a resident profile
        public async Task<ResidentResponse> LinkAccountAsync(
            int societyId,
            int residentProfileId,
            LinkResidentAccountRequest request)
        {
            await ValidateSocietyAccessAsync(societyId);

            var resident = await _context.ResidentProfiles
                .FirstOrDefaultAsync(r =>
                    r.ResidentProfileId == residentProfileId &&
                    r.SocietyId == societyId);

            if (resident == null)
                throw new KeyNotFoundException(
                    "Resident profile not found.");

            if (!resident.IsActive)
                throw new InvalidOperationException(
                    "Inactive resident profiles cannot be linked.");

            var user = await _userManager.FindByIdAsync(
                request.ApplicationUserId.Trim());

            if (user == null || !user.IsActive)
                throw new ArgumentException(
                    "Active application user not found.");

            if (user.SocietyId != societyId)
                throw new UnauthorizedAccessException(
                    "Application user does not belong to this society.");

            if (!await _userManager.IsInRoleAsync(user, "Resident"))
                throw new ArgumentException(
                    "Selected account does not have the Resident role.");

            if (!string.IsNullOrEmpty(resident.ApplicationUserId))
                throw new InvalidOperationException(
                    "This resident profile is already linked to an account.");

            var alreadyLinked = await _context.ResidentProfiles
                .AnyAsync(r => r.ApplicationUserId == user.Id);

            if (alreadyLinked)
                throw new InvalidOperationException(
                    "This application account is already linked to another resident.");

            resident.ApplicationUserId = user.Id;

            await _context.SaveChangesAsync();

            return MapResponse(resident);
        }

        private static ResidentResponse MapResponse(
            ResidentProfile resident)
        {
            return new ResidentResponse
            {
                ResidentProfileId = resident.ResidentProfileId,
                SocietyId = resident.SocietyId,
                FlatId = resident.FlatId,
                FullName = resident.FullName,
                MobileNumber = resident.MobileNumber,
                IsHome = resident.IsHome,
                IsActive = resident.IsActive,
                CreatedAt = resident.CreatedAt
            };
        }
    }
}
