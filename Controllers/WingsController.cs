
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using MTSS.DTOs.Wing;
using MTSS.Models;
using MTSS.Services;

namespace MTSS.Controllers
{
    [ApiController]
    [Route("api/societyadmin/wings")]
    [Authorize(Roles = "SocietyAdmin")]
    public class WingsController : ControllerBase
    {
        private readonly WingService _wingService;
        private readonly UserManager<ApplicationUser> _userManager;

        public WingsController(
            WingService wingService,
            UserManager<ApplicationUser> userManager)
        {
            _wingService = wingService;
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

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var societyId = await GetCurrentSocietyIdAsync();

            if (!societyId.HasValue)
                return Unauthorized(new
                {
                    success = false,
                    message = "The authenticated Society Admin account is inactive or invalid."
                });

            try
            {
                var data = await _wingService.GetAllAsync(societyId.Value);

                return Ok(new
                {
                    success = true,
                    message = "Wings retrieved successfully.",
                    data
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { success = false, message = ex.Message });
            }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var societyId = await GetCurrentSocietyIdAsync();

            if (!societyId.HasValue)
                return Unauthorized(new
                {
                    success = false,
                    message = "The authenticated Society Admin account is inactive or invalid."
                });

            try
            {
                var data = await _wingService.GetByIdAsync(societyId.Value, id);

                return Ok(new
                {
                    success = true,
                    message = "Wing retrieved successfully.",
                    data
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateWingRequest request)
        {
            var societyId = await GetCurrentSocietyIdAsync();

            if (!societyId.HasValue)
                return Unauthorized(new
                {
                    success = false,
                    message = "The authenticated Society Admin account is inactive or invalid."
                });

            try
            {
                var data = await _wingService.CreateAsync(societyId.Value, request);

                return StatusCode(StatusCodes.Status201Created, new
                {
                    success = true,
                    message = "Wing created successfully.",
                    data
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { success = false, message = ex.Message });
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

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] UpdateWingRequest request)
        {
            var societyId = await GetCurrentSocietyIdAsync();

            if (!societyId.HasValue)
                return Unauthorized(new
                {
                    success = false,
                    message = "The authenticated Society Admin account is inactive or invalid."
                });

            try
            {
                var data = await _wingService.UpdateAsync(
                    societyId.Value, id, request);

                return Ok(new
                {
                    success = true,
                    message = "Wing updated successfully.",
                    data
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { success = false, message = ex.Message });
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
        public async Task<IActionResult> SetStatus(
            int id,
            [FromBody] UpdateWingStatusRequest request)
        {
            var societyId = await GetCurrentSocietyIdAsync();

            if (!societyId.HasValue)
                return Unauthorized(new
                {
                    success = false,
                    message = "The authenticated Society Admin account is inactive or invalid."
                });

            try
            {
                var data = await _wingService.SetStatusAsync(
                    societyId.Value, id, request);

                return Ok(new
                {
                    success = true,
                    message = "Wing status updated successfully.",
                    data
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { success = false, message = ex.Message });
            }
        }
    }
}
