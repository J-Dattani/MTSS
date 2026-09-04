using Microsoft.EntityFrameworkCore;
using MTSS.Data;
using MTSS.DTOs.SubscriptionPlan;
using MTSS.Models;

namespace MTSS.Services
{
    public class SubscriptionPlanService
    {
        private readonly ApplicationDbContext _context;

        public SubscriptionPlanService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<SubscriptionPlan>> GetAllAsync()
        {
            return await _context.SubscriptionPlans
                .OrderBy(p => p.SubscriptionPlanId)
                .ToListAsync();
        }

        public async Task<SubscriptionPlan?> GetByIdAsync(int id)
        {
            return await _context.SubscriptionPlans
                .FirstOrDefaultAsync(p => p.SubscriptionPlanId == id);
        }

        //Buiness logic for creating a subscription plan
        public async Task<SubscriptionPlan> CreateAsync(CreateSubscriptionPlanRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Plan name is required.");

            if (request.MaxWings < 0 ||
                request.MaxFloorsPerWing < 0 ||
                request.MaxFlats < 0 ||
                request.MaxAmenities < 0 ||
                request.MaxSocietyAdmins < 0)
                throw new ArgumentException("Plan limits cannot be negative.");

            var plan = new SubscriptionPlan
            {
                Name = request.Name.Trim(),
                MaxWings = request.MaxWings,
                MaxFloorsPerWing = request.MaxFloorsPerWing,
                MaxFlats = request.MaxFlats,
                MaxAmenities = request.MaxAmenities,
                MaxSocietyAdmins = request.MaxSocietyAdmins,
                GuestApprovalEnabled = request.GuestApprovalEnabled,
                ParcelRegisterEnabled = request.ParcelRegisterEnabled,
                AmenityPinEnabled = request.AmenityPinEnabled,
                ComplaintManagementEnabled = request.ComplaintManagementEnabled,
                ReportsEnabled = request.ReportsEnabled,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.SubscriptionPlans.Add(plan);

            await _context.SaveChangesAsync();

            return plan;
        }

        //Business logic for updating a subscription plan
        public async Task<SubscriptionPlan> UpdateAsync(
    int id,
    SubscriptionPlan updatedPlan)
        {
            var existingPlan = await _context.SubscriptionPlans
                .FirstOrDefaultAsync(p => p.SubscriptionPlanId == id);

            if (existingPlan == null)
            {
                throw new KeyNotFoundException("Subscription plan not found.");
            }

            if (string.IsNullOrWhiteSpace(updatedPlan.Name))
            {
                throw new ArgumentException("Plan name is required.");
            }

            if (updatedPlan.MaxWings < 0 ||
                updatedPlan.MaxFloorsPerWing < 0 ||
                updatedPlan.MaxFlats < 0 ||
                updatedPlan.MaxAmenities < 0 ||
                updatedPlan.MaxSocietyAdmins < 0)
            {
                throw new ArgumentException("Plan limits cannot be negative.");
            }

            existingPlan.Name = updatedPlan.Name;
            existingPlan.MaxWings = updatedPlan.MaxWings;
            existingPlan.MaxFloorsPerWing = updatedPlan.MaxFloorsPerWing;
            existingPlan.MaxFlats = updatedPlan.MaxFlats;
            existingPlan.MaxAmenities = updatedPlan.MaxAmenities;
            existingPlan.MaxSocietyAdmins = updatedPlan.MaxSocietyAdmins;
            existingPlan.GuestApprovalEnabled = updatedPlan.GuestApprovalEnabled;
            existingPlan.ParcelRegisterEnabled = updatedPlan.ParcelRegisterEnabled;
            existingPlan.AmenityPinEnabled = updatedPlan.AmenityPinEnabled;
            existingPlan.ComplaintManagementEnabled = updatedPlan.ComplaintManagementEnabled;
            existingPlan.ReportsEnabled = updatedPlan.ReportsEnabled;

            await _context.SaveChangesAsync();

            return existingPlan;
        }

        //Business logic for setting the active status of a subscription plan
        public async Task<SubscriptionPlan> SetActiveStatusAsync(
    int id,
    bool isActive)
        {
            var existingPlan = await _context.SubscriptionPlans
                .FirstOrDefaultAsync(p => p.SubscriptionPlanId == id);

            if (existingPlan == null)
            {
                throw new KeyNotFoundException("Subscription plan not found.");
            }

            existingPlan.IsActive = isActive;

            await _context.SaveChangesAsync();

            return existingPlan;
        }
    }
}

