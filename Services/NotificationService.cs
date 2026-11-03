
using Microsoft.EntityFrameworkCore;
using MTSS.Data;
using MTSS.DTOs.Notification;
using MTSS.Models;

namespace MTSS.Services
{
    public class NotificationService
    {
        private readonly ApplicationDbContext _context;

        public NotificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        // SOCIETY ADMIN: Validate society
        private async Task ValidateSocietyAsync(int societyId)
        {
            var societyExists = await _context.Societies
                .AnyAsync(s =>
                    s.SocietyId == societyId &&
                    s.IsActive);

            if (!societyExists)
                throw new KeyNotFoundException(
                    "Active society not found.");
        }

        // SOCIETY ADMIN: Create notification

        public async Task<List<NotificationResponse>> CreateAsync(
            int societyId,
            CreateNotificationRequest request)
        {
            await ValidateSocietyAsync(societyId);

            IQueryable<ApplicationUser> recipients = _context.Users
                .Where(u =>
                    u.SocietyId == societyId &&
                    u.IsActive);

            // If UserId is supplied, target one resident.
            // Otherwise, target all active residents in this society.
            if (!string.IsNullOrWhiteSpace(request.UserId))
            {
                recipients = recipients.Where(u => u.Id == request.UserId);
            }

            var activeResidents = await (
                from user in recipients
                join profile in _context.ResidentProfiles
                    on user.Id equals profile.ApplicationUserId
                where profile.SocietyId == societyId &&
                      profile.IsActive
                select user
            )
            .Distinct()
            .ToListAsync();

            if (activeResidents.Count == 0)
                throw new KeyNotFoundException(
                    "No active resident found for the notification.");

            var notifications = activeResidents.Select(resident =>
                new Notification
                {
                    SocietyId = societyId,
                    UserId = resident.Id,
                    Title = request.Title.Trim(),
                    Message = request.Message.Trim(),
                    Type = request.Type.Trim(),
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                })
                .ToList();

            _context.Notifications.AddRange(notifications);
            await _context.SaveChangesAsync();

            return notifications.Select(MapResponse).ToList();
        }


        // RESIDENT: Get own notifications
        public async Task<List<NotificationResponse>> GetResidentNotificationsAsync(
            string applicationUserId)
        {
            var user = await GetActiveResidentAsync(applicationUserId);

            return await _context.Notifications
                .AsNoTracking()
                .Where(n =>
                    n.UserId == user.Id &&
                    n.SocietyId == user.SocietyId)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new NotificationResponse
                {
                    NotificationId = n.NotificationId,
                    SocietyId = n.SocietyId,
                    Title = n.Title,
                    Message = n.Message,
                    Type = n.Type,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt
                })
                .ToListAsync();
        }

        // RESIDENT: Get notification by ID
        public async Task<NotificationResponse> GetResidentNotificationByIdAsync(
            string applicationUserId,
            int notificationId)
        {
            var user = await GetActiveResidentAsync(applicationUserId);

            var notification = await _context.Notifications
                .AsNoTracking()
                .FirstOrDefaultAsync(n =>
                    n.NotificationId == notificationId &&
                    n.UserId == user.Id &&
                    n.SocietyId == user.SocietyId);

            if (notification == null)
                throw new KeyNotFoundException(
                    "Notification not found.");

            return MapResponse(notification);
        }

        // RESIDENT: Mark notification as read
        public async Task<NotificationReadResponse> MarkAsReadAsync(
            string applicationUserId,
            int notificationId)
        {
            var user = await GetActiveResidentAsync(applicationUserId);

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n =>
                    n.NotificationId == notificationId &&
                    n.UserId == user.Id &&
                    n.SocietyId == user.SocietyId);

            if (notification == null)
                throw new KeyNotFoundException(
                    "Notification not found.");

            notification.IsRead = true;

            await _context.SaveChangesAsync();

            return new NotificationReadResponse
            {
                NotificationId = notification.NotificationId,
                IsRead = notification.IsRead
            };
        }

        // RESIDENT: Mark all notifications as read
        public async Task<int> MarkAllAsReadAsync(
            string applicationUserId)
        {
            var user = await GetActiveResidentAsync(applicationUserId);

            var notifications = await _context.Notifications
                .Where(n =>
                    n.UserId == user.Id &&
                    n.SocietyId == user.SocietyId &&
                    !n.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();

            return notifications.Count;
        }

        // SOCIETY ADMIN: Get all society notifications
        public async Task<List<NotificationResponse>> GetAllAsync(
            int societyId)
        {
            await ValidateSocietyAsync(societyId);

            return await _context.Notifications
                .AsNoTracking()
                .Where(n => n.SocietyId == societyId)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new NotificationResponse
                {
                    NotificationId = n.NotificationId,
                    SocietyId = n.SocietyId,
                    Title = n.Title,
                    Message = n.Message,
                    Type = n.Type,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt
                })
                .ToListAsync();
        }

        // Validate authenticated resident account
        private async Task<ApplicationUser> GetActiveResidentAsync(
            string applicationUserId)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.Id == applicationUserId);

            if (user == null || !user.IsActive ||
                user.SocietyId == null)
            {
                throw new UnauthorizedAccessException(
                    "Resident account is inactive or invalid.");
            }

            var hasResidentProfile = await _context.ResidentProfiles
                .AnyAsync(r =>
                    r.ApplicationUserId == user.Id &&
                    r.SocietyId == user.SocietyId &&
                    r.IsActive);

            if (!hasResidentProfile)
                throw new UnauthorizedAccessException(
                    "No active resident profile is linked to this account.");

            return user;
        }

        private static NotificationResponse MapResponse(
            Notification notification)
        {
            return new NotificationResponse
            {
                NotificationId = notification.NotificationId,
                SocietyId = notification.SocietyId,
                Title = notification.Title,
                Message = notification.Message,
                Type = notification.Type,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt
            };
        }
    }
}
