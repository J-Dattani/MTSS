
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MTSS.DTOs.Flat;
using MTSS.Models;
using MTSS.Services;

namespace MTSS.Controllers
{
    [ApiController]
    [Route("api/societyadmin/flats")]
    [Authorize(Roles = "SocietyAdmin")]
    public class FlatsController : ControllerBase
    {
        private readonly FlatService _flatService;
        private readonly UserManager<ApplicationUser> _userManager;

        public FlatsController(
            FlatService flatService,
            UserManager<ApplicationUser> userManager)
        {
            _flatService = flatService;
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
                var data = await _flatService.GetAllAsync(societyId.Value);

                return Ok(new
                {
                    success = true,
                    message = "Flats retrieved successfully.",
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
                var data = await _flatService.GetByIdAsync(societyId.Value, id);

                return Ok(new
                {
                    success = true,
                    message = "Flat retrieved successfully.",
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
            [FromBody] CreateFlatRequest request)
        {
            var societyId = await GetCurrentSocietyIdAsync();

            if (!societyId.HasValue)
                return InvalidAdmin();

            try
            {
                var data = await _flatService.CreateAsync(societyId.Value, request);

                return StatusCode(StatusCodes.Status201Created, new
                {
                    success = true,
                    message = "Flat created successfully.",
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

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] UpdateFlatRequest request)
        {
            var societyId = await GetCurrentSocietyIdAsync();

            if (!societyId.HasValue)
                return InvalidAdmin();

            try
            {
                var data = await _flatService.UpdateAsync(
                    societyId.Value, id, request);

                return Ok(new
                {
                    success = true,
                    message = "Flat updated successfully.",
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
        public async Task<IActionResult> SetStatus(
            int id,
            [FromBody] UpdateFlatStatusRequest request)
        {
            var societyId = await GetCurrentSocietyIdAsync();

            if (!societyId.HasValue)
                return InvalidAdmin();

            try
            {
                var data = await _flatService.SetStatusAsync(
                    societyId.Value, id, request);

                return Ok(new
                {
                    success = true,
                    message = "Flat status updated successfully.",
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
