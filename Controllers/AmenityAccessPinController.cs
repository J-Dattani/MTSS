
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MTSS.DTOs.AmenityAccessPin;
using MTSS.Models;
using MTSS.Services;

namespace MTSS.Controllers
{
    [ApiController]
    [Route("api")]
    public class AmenityAccessPinController : ControllerBase
    {
        private readonly AmenityAccessPinService _service;
        private readonly UserManager<ApplicationUser> _userManager;

        public AmenityAccessPinController(
            AmenityAccessPinService service,
            UserManager<ApplicationUser> userManager)
        {
            _service = service;
            _userManager = userManager;
        }

        // RESIDENT: Generate PIN
        [Authorize(Roles = "Resident")]
        [HttpPost("resident/amenity-pins")]
        public async Task<IActionResult> Generate(
            [FromBody] GenerateAmenityPinRequest request)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                if (user == null || !user.IsActive)
                    return Unauthorized(new
                    {
                        success = false,
                        message = "Invalid or inactive resident account."
                    });

                var result = await _service.GenerateAsync(user.Id, request);

                return Ok(new
                {
                    success = true,
                    message = "Amenity PIN generated successfully.",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        // RESIDENT: Get own PINs
        [Authorize(Roles = "Resident")]
        [HttpGet("resident/amenity-pins")]
        public async Task<IActionResult> GetResidentPins()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                if (user == null || !user.IsActive)
                    return Unauthorized(new
                    {
                        success = false,
                        message = "Invalid or inactive resident account."
                    });

                var result = await _service.GetResidentPinsAsync(user.Id);

                return Ok(new
                {
                    success = true,
                    message = "Amenity PIN records retrieved successfully.",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        // RESIDENT: Get own PIN by ID
        [Authorize(Roles = "Resident")]
        [HttpGet("resident/amenity-pins/{id:int}")]
        public async Task<IActionResult> GetResidentPinById(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                if (user == null || !user.IsActive)
                    return Unauthorized(new
                    {
                        success = false,
                        message = "Invalid or inactive resident account."
                    });

                var result = await _service.GetResidentPinByIdAsync(
                    user.Id, id);

                return Ok(new
                {
                    success = true,
                    message = "Amenity PIN retrieved successfully.",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        // RESIDENT: Revoke unused PIN
        [Authorize(Roles = "Resident")]
        [HttpPatch("resident/amenity-pins/{id:int}/revoke")]
        public async Task<IActionResult> Revoke(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                if (user == null || !user.IsActive)
                    return Unauthorized(new
                    {
                        success = false,
                        message = "Invalid or inactive resident account."
                    });

                var result = await _service.RevokeAsync(user.Id, id);

                return Ok(new
                {
                    success = true,
                    message = "Amenity PIN revoked successfully.",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        // SOCIETY ADMIN: Get all society PINs
        [Authorize(Roles = "SocietyAdmin")]
        [HttpGet("societyadmin/amenity-pins")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                if (user == null || !user.IsActive ||
                    user.SocietyId == null)
                {
                    return Unauthorized(new
                    {
                        success = false,
                        message = "Invalid or inactive society admin account."
                    });
                }

                var result = await _service.GetAllAsync(user.SocietyId.Value);

                return Ok(new
                {
                    success = true,
                    message = "Society amenity PIN records retrieved successfully.",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        // SOCIETY ADMIN: Get amenity access logs
        [Authorize(Roles = "SocietyAdmin")]
        [HttpGet("societyadmin/amenity-access-logs")]
        public async Task<IActionResult> GetAccessLogs()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                if (user == null || !user.IsActive ||
                    user.SocietyId == null)
                {
                    return Unauthorized(new
                    {
                        success = false,
                        message = "Invalid or inactive society admin account."
                    });
                }

                var result = await _service.GetAccessLogsAsync(
                    user.SocietyId.Value);

                return Ok(new
                {
                    success = true,
                    message = "Amenity access logs retrieved successfully.",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        // SOCIETY ADMIN: Validate PIN at amenity entrance
        [Authorize(Roles = "SocietyAdmin")]
        [HttpPost("societyadmin/amenity-pins/validate")]
        public async Task<IActionResult> ValidatePin(
            [FromBody] ValidateAmenityPinRequest request)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                if (user == null || !user.IsActive ||
                    user.SocietyId == null)
                {
                    return Unauthorized(new
                    {
                        success = false,
                        message = "Invalid or inactive society admin account."
                    });
                }

                var result = await _service.ValidatePinAsync(
                    user.SocietyId.Value, request);

                return Ok(new
                {
                    success = true,
                    message = result.Message,
                    data = result
                });
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        // Common API exception handling
        private IActionResult HandleException(Exception ex)
        {
            if (ex is KeyNotFoundException)
                return NotFound(new
                {
                    success = false,
                    message = ex.Message
                });

            if (ex is UnauthorizedAccessException)
                return StatusCode(403, new
                {
                    success = false,
                    message = ex.Message
                });

            if (ex is InvalidOperationException ||
                ex is ArgumentException)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }

            return StatusCode(500, new
            {
                success = false,
                message = "An unexpected error occurred."
            });
        }
    }
}
