
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MTSS.DTOs.Notification;
using MTSS.Models;
using MTSS.Services;

namespace MTSS.Controllers
{
    [ApiController]
    [Route("api")]
    public class NotificationController : ControllerBase
    {
        private readonly NotificationService _notificationService;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationController(
            NotificationService notificationService,
            UserManager<ApplicationUser> userManager)
        {
            _notificationService = notificationService;
            _userManager = userManager;
        }

        private string GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("User not authenticated.");
        }

        private async Task<ApplicationUser> GetCurrentSocietyAdminAsync()
        {
            var user = await _userManager.FindByIdAsync(GetCurrentUserId());

            if (user == null || !user.IsActive || user.SocietyId == null)
                throw new UnauthorizedAccessException("Invalid or inactive SocietyAdmin account.");

            return user;
        }

        // RESIDENT: Get all own notifications
        [Authorize(Roles = "Resident")]
        [HttpGet("resident/notifications")]
        public async Task<IActionResult> GetMyNotifications()
        {
            try
            {
                var result = await _notificationService
                    .GetResidentNotificationsAsync(GetCurrentUserId());

                return Ok(new
                {
                    success = true,
                    message = "Notifications retrieved successfully.",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        // RESIDENT: Get notification by ID
        [Authorize(Roles = "Resident")]
        [HttpGet("resident/notifications/{id:int}")]
        public async Task<IActionResult> GetNotificationById(int id)
        {
            try
            {
                var result = await _notificationService
                    .GetResidentNotificationByIdAsync(GetCurrentUserId(), id);

                return Ok(new
                {
                    success = true,
                    message = "Notification retrieved successfully.",
                    data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // RESIDENT: Mark one notification as read
        [Authorize(Roles = "Resident")]
        [HttpPatch("resident/notifications/{id:int}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            try
            {
                var result = await _notificationService
                    .MarkAsReadAsync(GetCurrentUserId(), id);

                return Ok(new
                {
                    success = true,
                    message = "Notification marked as read.",
                    data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // RESIDENT: Mark all notifications as read
        [Authorize(Roles = "Resident")]
        [HttpPatch("resident/notifications/read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            try
            {
                var count = await _notificationService
                    .MarkAllAsReadAsync(GetCurrentUserId());

                return Ok(new
                {
                    success = true,
                    message = "All notifications marked as read.",
                    data = new { updatedCount = count }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // SOCIETY ADMIN: Get all society notifications
        [Authorize(Roles = "SocietyAdmin")]
        [HttpGet("societyadmin/notifications")]
        public async Task<IActionResult> GetSocietyNotifications()
        {
            try
            {
                var admin = await GetCurrentSocietyAdminAsync();

                var result = await _notificationService
                    .GetAllAsync(admin.SocietyId!.Value);

                return Ok(new
                {
                    success = true,
                    message = "Society notifications retrieved successfully.",
                    data = result
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // SOCIETY ADMIN: Create notification for a resident
        [Authorize(Roles = "SocietyAdmin")]
        [HttpPost("societyadmin/notifications")]
        public async Task<IActionResult> CreateNotification(
            [FromBody] CreateNotificationRequest request)
        {
            try
            {
                var admin = await GetCurrentSocietyAdminAsync();

                var result = await _notificationService.CreateAsync(
                    admin.SocietyId!.Value,
                    request
                );

                return Ok(new
                {
                    success = true,
                    message = $"{result.Count} notification(s) created successfully.",
                    data = result
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
