using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MTSS.DTOs.SocietyAdmin;
using MTSS.Services;

namespace MTSS.Controllers
{
    [ApiController]
    [Route("api/superadmin/societies/{societyId}/admins")]
    [Authorize(Roles = "SuperAdmin")]
    public class SocietyAdminsController : ControllerBase
    {
        private readonly SocietyAdminService _societyAdminService;

        public SocietyAdminsController(
            SocietyAdminService societyAdminService)
        {
            _societyAdminService = societyAdminService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int societyId)
        {
            try
            {
                var admins =
                    await _societyAdminService.GetAllAsync(societyId);

                return Ok(new
                {
                    success = true,
                    message = "Society Admins retrieved successfully.",
                    data = admins
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetById(
            int societyId,
            string userId)
        {
            try
            {
                var admin =
                    await _societyAdminService.GetByIdAsync(
                        societyId,
                        userId);

                return Ok(new
                {
                    success = true,
                    message = "Society Admin retrieved successfully.",
                    data = admin
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            int societyId,
            [FromBody] CreateSocietyAdminRequest request)
        {
            try
            {
                var admin =
                    await _societyAdminService.CreateAsync(
                        societyId,
                        request);

                return StatusCode(StatusCodes.Status201Created, new
                {
                    success = true,
                    message = "Society Admin created successfully.",
                    data = admin
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpPut("{userId}")]
        public async Task<IActionResult> Update(
            int societyId,
            string userId,
            [FromBody] UpdateSocietyAdminRequest request)
        {
            try
            {
                var admin =
                    await _societyAdminService.UpdateAsync(
                        societyId,
                        userId,
                        request);

                return Ok(new
                {
                    success = true,
                    message = "Society Admin updated successfully.",
                    data = admin
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpPatch("{userId}/status")]
        public async Task<IActionResult> SetStatus(
            int societyId,
            string userId,
            [FromBody] UpdateSocietyAdminStatusRequest request)
        {
            try
            {
                var admin =
                    await _societyAdminService.SetStatusAsync(
                        societyId,
                        userId,
                        request);

                return Ok(new
                {
                    success = true,
                    message = "Society Admin status updated successfully.",
                    data = admin
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
    }
}