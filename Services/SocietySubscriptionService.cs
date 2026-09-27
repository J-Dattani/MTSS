using Microsoft.EntityFrameworkCore;
using MTSS.Data;
using MTSS.DTOs.SocietySubscription;
using MTSS.Models;

namespace MTSS.Services
{
    public class SocietySubscriptionService
    {
        private readonly ApplicationDbContext _context;

        public SocietySubscriptionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SocietySubscription?> GetBySocietyIdAsync(
            int societyId)
        {
            return await _context.SocietySubscriptions
                .Where(s => s.SocietyId == societyId)
                .OrderByDescending(s => s.SocietySubscriptionId)
                .FirstOrDefaultAsync();
        }

        public async Task<SocietySubscription> AssignAsync(
            int societyId,
            AssignSocietySubscriptionRequest request)
        {
            var society = await _context.Societies
                .FirstOrDefaultAsync(s => s.SocietyId == societyId);

            if (society == null)
            {
                throw new KeyNotFoundException("Society not found.");
            }

            if (!society.IsActive)
            {
                throw new InvalidOperationException(
                    "Cannot assign a subscription to an inactive society.");
            }

            var plan = await _context.SubscriptionPlans
                .FirstOrDefaultAsync(p =>
                    p.SubscriptionPlanId == request.SubscriptionPlanId);

            if (plan == null)
            {
                throw new KeyNotFoundException(
                    "Subscription plan not found.");
            }

            if (!plan.IsActive)
            {
                throw new InvalidOperationException(
                    "Cannot assign an inactive subscription plan.");
            }

            ValidateDates(request.StartDate, request.EndDate);

            var hasActiveSubscription = await _context.SocietySubscriptions
                .AnyAsync(s =>
                    s.SocietyId == societyId &&
                    s.IsActive);

            if (hasActiveSubscription)
            {
                throw new InvalidOperationException(
                    "Society already has an active subscription.");
            }

            var subscription = new SocietySubscription
            {
                SocietyId = societyId,
                SubscriptionPlanId = request.SubscriptionPlanId,
                Status = "Active",
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.SocietySubscriptions.Add(subscription);
            await _context.SaveChangesAsync();

            return subscription;
        }

        public async Task<SocietySubscription> UpdateAsync(
            int societyId,
            UpdateSocietySubscriptionRequest request)
        {
            var subscription = await _context.SocietySubscriptions
                .FirstOrDefaultAsync(s =>
                    s.SocietyId == societyId &&
                    s.IsActive);

            if (subscription == null)
            {
                throw new KeyNotFoundException(
                    "Active society subscription not found.");
            }

            var society = await _context.Societies
                .FirstOrDefaultAsync(s => s.SocietyId == societyId);

            if (society == null)
            {
                throw new KeyNotFoundException("Society not found.");
            }

            if (!society.IsActive)
            {
                throw new InvalidOperationException(
                    "Cannot update the subscription of an inactive society.");
            }

            var plan = await _context.SubscriptionPlans
                .FirstOrDefaultAsync(p =>
                    p.SubscriptionPlanId == request.SubscriptionPlanId);

            if (plan == null)
            {
                throw new KeyNotFoundException(
                    "Subscription plan not found.");
            }

            if (!plan.IsActive)
            {
                throw new InvalidOperationException(
                    "Cannot assign an inactive subscription plan.");
            }

            ValidateDates(request.StartDate, request.EndDate);

            subscription.SubscriptionPlanId =
                request.SubscriptionPlanId;

            subscription.StartDate = request.StartDate;
            subscription.EndDate = request.EndDate;

            await _context.SaveChangesAsync();

            return subscription;
        }

        public async Task<SocietySubscription> UpdateStatusAsync(
            int societyId,
            UpdateSocietySubscriptionStatusRequest request)
        {
            var subscription = await _context.SocietySubscriptions
                .FirstOrDefaultAsync(s =>
                    s.SocietyId == societyId &&
                    s.IsActive);

            if (subscription == null)
            {
                throw new KeyNotFoundException(
                    "Active society subscription not found.");
            }

            var status = request.Status.Trim();

            var allowedStatuses = new[]
            {
                "Trial",
                "Active",
                "Suspended",
                "Expired"
            };

            if (!allowedStatuses.Contains(
                status,
                StringComparer.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    "Invalid subscription status. " +
                    "Allowed values are Trial, Active, Suspended, or Expired.");
            }

            subscription.Status =
                allowedStatuses.First(s =>
                    s.Equals(status, StringComparison.OrdinalIgnoreCase));

            subscription.IsActive = request.IsActive;

            await _context.SaveChangesAsync();

            return subscription;
        }

        private static void ValidateDates(
            DateTime startDate,
            DateTime endDate)
        {
            if (startDate >= endDate)
            {
                throw new ArgumentException(
                    "Subscription start date must be before end date.");
            }
        }
    }
}