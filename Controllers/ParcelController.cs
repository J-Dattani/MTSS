
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MTSS.DTOs.Parcel;
using MTSS.Models;
using MTSS.Services;

namespace MTSS.Controllers
{
    [Route("api/societyadmin/parcels")]
    [ApiController]
    [Authorize(Roles = "SocietyAdmin")]
    public class ParcelController : ControllerBase
    {
        private readonly ParcelService _parcelService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ParcelController(
            ParcelService parcelService,
            UserManager<ApplicationUser> userManager)
        {
            _parcelService = parcelService;
            _userManager = userManager;
        }

        private async Task<int> GetSocietyIdAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null || !user.IsActive || user.SocietyId == null)
                throw new UnauthorizedAccessException(
                    "Active SocietyAdmin account required.");

            return user.SocietyId.Value;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var societyId = await GetSocietyIdAsync();
                var parcels = await _parcelService.GetAllAsync(societyId);

                return Ok(new
                {
                    success = true,
                    message = "Parcels retrieved successfully.",
                    data = parcels
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { success = false, message = ex.Message });
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

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var societyId = await GetSocietyIdAsync();
                var parcel = await _parcelService.GetByIdAsync(societyId, id);

                return Ok(new
                {
                    success = true,
                    message = "Parcel retrieved successfully.",
                    data = parcel
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { success = false, message = ex.Message });
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

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateParcelRequest request)
        {
            try
            {
                var societyId = await GetSocietyIdAsync();
                var parcel = await _parcelService.CreateAsync(societyId, request);

                return StatusCode(201, new
                {
                    success = true,
                    message = "Parcel registered successfully.",
                    data = parcel
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { success = false, message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
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
            int id, [FromBody] UpdateParcelStatusRequest request)
        {
            try
            {
                var societyId = await GetSocietyIdAsync();
                var parcel = await _parcelService.UpdateStatusAsync(
                    societyId, id, request);

                return Ok(new
                {
                    success = true,
                    message = "Parcel status updated successfully.",
                    data = parcel
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { success = false, message = ex.Message });
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
    }
}
