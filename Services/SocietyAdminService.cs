using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MTSS.Data;
using MTSS.DTOs.SocietyAdmin;
using MTSS.Models;

namespace MTSS.Services
{
    public class SocietyAdminService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SocietyAdminService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<List<SocietyAdminResponse>> GetAllAsync(int societyId)
        {
            var society = await _context.Societies
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.SocietyId == societyId);

            if (society == null)
                throw new KeyNotFoundException("Society not found.");

            var admins = await _userManager.GetUsersInRoleAsync("SocietyAdmin");

            return admins
                .Where(u => u.SocietyId == societyId)
                .Select(MapToResponse)
                .ToList();
        }

        public async Task<SocietyAdminResponse> GetByIdAsync(
            int societyId,
            string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null ||
                user.SocietyId != societyId ||
                !await _userManager.IsInRoleAsync(user, "SocietyAdmin"))
            {
                throw new KeyNotFoundException("Society Admin not found.");
            }

            return MapToResponse(user);
        }

        public async Task<SocietyAdminResponse> CreateAsync(
            int societyId,
            CreateSocietyAdminRequest request)
        {
            var society = await _context.Societies
                .FirstOrDefaultAsync(s => s.SocietyId == societyId);

            if (society == null)
                throw new KeyNotFoundException("Society not found.");

            if (!society.IsActive)
                throw new InvalidOperationException(
                    "Cannot create Society Admin for an inactive society.");

            var subscription = await _context.SocietySubscriptions
                .Include(s => s.SubscriptionPlan)
                .Where(s =>
                    s.SocietyId == societyId &&
                    s.IsActive &&
                    s.EndDate > DateTime.UtcNow &&
                    (s.Status == "Trial" || s.Status == "Active"))
                .OrderByDescending(s => s.SocietySubscriptionId)
                .FirstOrDefaultAsync();

            if (subscription == null)
                throw new InvalidOperationException(
                    "Society does not have an active subscription.");

            if (subscription.SubscriptionPlan == null)
                throw new InvalidOperationException(
                    "The society's subscription plan could not be found.");

            if (!subscription.SubscriptionPlan.IsActive)
                throw new InvalidOperationException(
                    "The society's subscription plan is inactive.");

            var admins = await _userManager.GetUsersInRoleAsync("SocietyAdmin");

            var activeAdminCount = admins.Count(u =>
                u.SocietyId == societyId &&
                u.IsActive);

            if (activeAdminCount >= subscription.SubscriptionPlan.MaxSocietyAdmins)
            {
                throw new InvalidOperationException(
                    "Society Admin quota has been reached.");
            }

            var existingUser =
                await _userManager.FindByEmailAsync(request.Email);

            if (existingUser != null)
                throw new InvalidOperationException(
                    "A user with this email already exists.");

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FullName = request.FullName,
                SocietyId = societyId,
                IsActive = true,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(
                user,
                request.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(
                    "; ",
                    result.Errors.Select(e => e.Description));

                throw new ArgumentException(errors);
            }

            var roleResult = await _userManager.AddToRoleAsync(
                user,
                "SocietyAdmin");

            if (!roleResult.Succeeded)
            {
                var errors = string.Join(
                    "; ",
                    roleResult.Errors.Select(e => e.Description));

                await _userManager.DeleteAsync(user);

                throw new InvalidOperationException(
                    $"Society Admin role assignment failed: {errors}");
            }

            return MapToResponse(user);
        }

        public async Task<SocietyAdminResponse> UpdateAsync(
            int societyId,
            string userId,
            UpdateSocietyAdminRequest request)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null ||
                user.SocietyId != societyId ||
                !await _userManager.IsInRoleAsync(user, "SocietyAdmin"))
            {
                throw new KeyNotFoundException("Society Admin not found.");
            }

            if (!string.Equals(
                    user.Email,
                    request.Email,
                    StringComparison.OrdinalIgnoreCase))
            {
                var existingUser =
                    await _userManager.FindByEmailAsync(request.Email);

                if (existingUser != null &&
                    existingUser.Id != user.Id)
                {
                    throw new InvalidOperationException(
                        "A user with this email already exists.");
                }

                var emailResult = await _userManager.SetEmailAsync(
                    user,
                    request.Email);

                if (!emailResult.Succeeded)
                {
                    var errors = string.Join(
                        "; ",
                        emailResult.Errors.Select(e => e.Description));

                    throw new ArgumentException(errors);
                }

                var usernameResult = await _userManager.SetUserNameAsync(
                    user,
                    request.Email);

                if (!usernameResult.Succeeded)
                {
                    var errors = string.Join(
                        "; ",
                        usernameResult.Errors.Select(e => e.Description));

                    throw new ArgumentException(errors);
                }
            }

            user.FullName = request.FullName;

            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                var errors = string.Join(
                    "; ",
                    updateResult.Errors.Select(e => e.Description));

                throw new ArgumentException(errors);
            }

            return MapToResponse(user);
        }

        public async Task<SocietyAdminResponse> SetStatusAsync(
            int societyId,
            string userId,
            UpdateSocietyAdminStatusRequest request)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null ||
                user.SocietyId != societyId ||
                !await _userManager.IsInRoleAsync(user, "SocietyAdmin"))
            {
                throw new KeyNotFoundException("Society Admin not found.");
            }

            user.IsActive = request.IsActive;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                var errors = string.Join(
                    "; ",
                    result.Errors.Select(e => e.Description));

                throw new ArgumentException(errors);
            }

            return MapToResponse(user);
        }

        private static SocietyAdminResponse MapToResponse(
            ApplicationUser user)
        {
            return new SocietyAdminResponse
            {
                UserId = user.Id,
                SocietyId = user.SocietyId ?? 0,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                IsActive = user.IsActive
            };
        }
    }
}