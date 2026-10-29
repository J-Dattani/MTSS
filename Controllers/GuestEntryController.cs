
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MTSS.DTOs.GuestEntry;
using MTSS.Models;
using MTSS.Services;

namespace MTSS.Controllers
{
    [ApiController]
    [Route("api/societyadmin/guest-entries")]
    [Authorize(Roles = "SocietyAdmin")]
    public class GuestEntryController : ControllerBase
    {
        private readonly GuestEntryService _guestEntryService;
        private readonly UserManager<ApplicationUser> _userManager;

        public GuestEntryController(
            GuestEntryService guestEntryService,
            UserManager<ApplicationUser> userManager)
        {
            _guestEntryService = guestEntryService;
            _userManager = userManager;
        }

        private async Task<int?> GetCurrentSocietyIdAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
                return null;

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null || !user.IsActive || !user.SocietyId.HasValue)
                return null;

            return user.SocietyId.Value;
        }

        private IActionResult InvalidAdmin()
        {
            return Unauthorized(new
            {
                success = false,
                message = "The authenticated Society Admin account is inactive or invalid."
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var societyId = await GetCurrentSocietyIdAsync();

            if (!societyId.HasValue)
                return InvalidAdmin();

            try
            {
                var data = await _guestEntryService.GetAllAsync(societyId.Value);

                return Ok(new
                {
                    success = true,
                    message = "Guest entry requests retrieved successfully.",
                    data
                });
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

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var societyId = await GetCurrentSocietyIdAsync();

            if (!societyId.HasValue)
                return InvalidAdmin();

            try
            {
                var data = await _guestEntryService.GetByIdAsync(
                    societyId.Value, id);

                return Ok(new
                {
                    success = true,
                    message = "Guest entry request retrieved successfully.",
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

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateGuestEntryRequest request)
        {
            var societyId = await GetCurrentSocietyIdAsync();

            if (!societyId.HasValue)
                return InvalidAdmin();

            try
            {
                var data = await _guestEntryService.CreateAsync(
                    societyId.Value, request);

                return StatusCode(StatusCodes.Status201Created, new
                {
                    success = true,
                    message = "Guest entry request created successfully.",
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
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPatch("{id:int}/status")]
        public async Task<IActionResult> UpdateStatus(
            int id,
            [FromBody] UpdateGuestEntryStatusRequest request)
        {
            var societyId = await GetCurrentSocietyIdAsync();

            if (!societyId.HasValue)
                return InvalidAdmin();

            try
            {
                var data = await _guestEntryService.UpdateStatusAsync(
                    societyId.Value, id, request);

                return Ok(new
                {
                    success = true,
                    message = "Guest entry request status updated successfully.",
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
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
