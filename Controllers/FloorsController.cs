
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using MTSS.DTOs.Floor;
using MTSS.Models;
using MTSS.Services;

namespace MTSS.Controllers
{
    [ApiController]
    [Route("api/societyadmin/floors")]
    [Authorize(Roles = "SocietyAdmin")]
    public class FloorsController : ControllerBase
    {
        private readonly FloorService _floorService;
        private readonly UserManager<ApplicationUser> _userManager;

        public FloorsController(
            FloorService floorService,
            UserManager<ApplicationUser> userManager)
        {
            _floorService = floorService;
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
                var data = await _floorService.GetAllAsync(societyId.Value);

                return Ok(new
                {
                    success = true,
                    message = "Floors retrieved successfully.",
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
                var data = await _floorService.GetByIdAsync(societyId.Value, id);

                return Ok(new
                {
                    success = true,
                    message = "Floor retrieved successfully.",
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

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateFloorRequest request)
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
                var data = await _floorService.CreateAsync(societyId.Value, request);

                return StatusCode(StatusCodes.Status201Created, new
                {
                    success = true,
                    message = "Floor created successfully.",
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
            [FromBody] UpdateFloorRequest request)
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
                var data = await _floorService.UpdateAsync(
                    societyId.Value, id, request);

                return Ok(new
                {
                    success = true,
                    message = "Floor updated successfully.",
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
            [FromBody] UpdateFloorStatusRequest request)
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
                var data = await _floorService.SetStatusAsync(
                    societyId.Value, id, request);

                return Ok(new
                {
                    success = true,
                    message = "Floor status updated successfully.",
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
