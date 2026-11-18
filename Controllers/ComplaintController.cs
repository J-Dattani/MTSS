
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MTSS.DTOs.Complaint;
using MTSS.Models;
using MTSS.Services;

namespace MTSS.Controllers
{
    [ApiController]
    [Route("api")]
    public class ComplaintController : ControllerBase
    {
        private readonly ComplaintService _complaintService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ComplaintController(
            ComplaintService complaintService,
            UserManager<ApplicationUser> userManager)
        {
            _complaintService = complaintService;
            _userManager = userManager;
        }

        private string? GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        private async Task<int?> GetCurrentSocietyIdAsync()
        {
            var userId = GetCurrentUserId();

            if (string.IsNullOrWhiteSpace(userId))
                return null;

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null ||
                !user.IsActive ||
                !user.SocietyId.HasValue)
                return null;

            return user.SocietyId.Value;
        }

        private IActionResult InvalidAccount()
        {
            return Unauthorized(new
            {
                success = false,
                message = "The authenticated account is inactive or invalid."
            });
        }

        // RESIDENT: Submit complaint
        [Authorize(Roles = "Resident")]
        [HttpPost("resident/complaints")]
        public async Task<IActionResult> CreateResidentComplaint(
            [FromBody] CreateComplaintRequest request)
        {
            var userId = GetCurrentUserId();

            if (string.IsNullOrWhiteSpace(userId))
                return InvalidAccount();

            try
            {
                var data = await _complaintService.CreateForResidentAsync(
                    userId, request);

                return StatusCode(StatusCodes.Status201Created, new
                {
                    success = true,
                    message = "Complaint submitted successfully.",
                    data
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { success = false, message = ex.Message });
            }
        }

        // RESIDENT: Get own complaints
        [Authorize(Roles = "Resident")]
        [HttpGet("resident/complaints")]
        public async Task<IActionResult> GetResidentComplaints()
        {
            var userId = GetCurrentUserId();

            if (string.IsNullOrWhiteSpace(userId))
                return InvalidAccount();

            try
            {
                var data = await _complaintService.GetResidentComplaintsAsync(
                    userId);

                return Ok(new
                {
                    success = true,
                    message = "Your complaints retrieved successfully.",
                    data
                });
            }   
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { success = false, message = ex.Message });
            }
        }

        // RESIDENT: Get own complaint by ID
        [Authorize(Roles = "Resident")]
        [HttpGet("resident/complaints/{id:int}")]
        public async Task<IActionResult> GetResidentComplaintById(int id)
        {
            var userId = GetCurrentUserId();

            if (string.IsNullOrWhiteSpace(userId))
                return InvalidAccount();

            try
            {
                var data = await _complaintService.GetResidentComplaintByIdAsync(
                    userId, id);

                return Ok(new
                {
                    success = true,
                    message = "Complaint retrieved successfully.",
                    data
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { success = false, message = ex.Message });
            }
        }

        // SOCIETY ADMIN: Get all society complaints
        [Authorize(Roles = "SocietyAdmin")]
        [HttpGet("societyadmin/complaints")]
        public async Task<IActionResult> GetAllComplaints()
        {
            var societyId = await GetCurrentSocietyIdAsync();

            if (!societyId.HasValue)
                return InvalidAccount();

            try
            {
                var data = await _complaintService.GetAllAsync(
                    societyId.Value);

                return Ok(new
                {
                    success = true,
                    message = "Complaints retrieved successfully.",
                    data
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { success = false, message = ex.Message });
            }
        }

        // SOCIETY ADMIN: Get complaint by ID
        [Authorize(Roles = "SocietyAdmin")]
        [HttpGet("societyadmin/complaints/{id:int}")]
        public async Task<IActionResult> GetComplaintById(int id)
        {
            var societyId = await GetCurrentSocietyIdAsync();

            if (!societyId.HasValue)
                return InvalidAccount();

            try
            {
                var data = await _complaintService.GetByIdAsync(
                    societyId.Value, id);

                return Ok(new
                {
                    success = true,
                    message = "Complaint retrieved successfully.",
                    data
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { success = false, message = ex.Message });
            }
        }

        // SOCIETY ADMIN: Update complaint status
        [Authorize(Roles = "SocietyAdmin")]
        [HttpPatch("societyadmin/complaints/{id:int}/status")]
        public async Task<IActionResult> UpdateComplaintStatus(
            int id,
            [FromBody] UpdateComplaintStatusRequest request)
        {
            var societyId = await GetCurrentSocietyIdAsync();

            if (!societyId.HasValue)
                return InvalidAccount();

            try
            {
                var data = await _complaintService.UpdateStatusAsync(
                    societyId.Value, id, request);

                return Ok(new
                {
                    success = true,
                    message = "Complaint status updated successfully.",
                    data
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { success = false, message = ex.Message });
            }
        }
    }
}
