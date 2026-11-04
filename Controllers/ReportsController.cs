
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MTSS.Models;
using MTSS.Services;

namespace MTSS.Controllers
{
    [ApiController]
    [Route("api/societyadmin")]
    [Authorize(Roles = "SocietyAdmin")]
    public class ReportsController : ControllerBase
    {
        private readonly ReportService _reportService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReportsController(
            ReportService reportService,
            UserManager<ApplicationUser> userManager)
        {
            _reportService = reportService;
            _userManager = userManager;
        }

        // SocietyAdmin: Get Society Dashboard Summary
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
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

                var result = await _reportService.GetDashboardAsync(
                    user.SocietyId.Value);

                return Ok(new
                {
                    success = true,
                    message = "Society dashboard retrieved successfully.",
                    data = result
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
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An unexpected error occurred."
                });
            }
        }

        // SocietyAdmin: Get Resident Report
        [HttpGet("reports/residents")]
        public async Task<IActionResult> GetResidentsReport()
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

                var result = await _reportService.GetResidentsReportAsync(
                    user.SocietyId.Value);

                return Ok(new
                {
                    success = true,
                    message = "Resident report retrieved successfully.",
                    data = result
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
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An unexpected error occurred."
                });
            }
        }

        // SocietyAdmin: Get Guest Entry Report
        [HttpGet("reports/guest-entries")]
        public async Task<IActionResult> GetGuestEntriesReport()
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

                var result = await _reportService.GetGuestEntriesReportAsync(
                    user.SocietyId.Value);

                return Ok(new
                {
                    success = true,
                    message = "Guest entry report retrieved successfully.",
                    data = result
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
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An unexpected error occurred."
                });
            }
        }

        // SocietyAdmin: Get Parcel Report
        [HttpGet("reports/parcels")]
        public async Task<IActionResult> GetParcelsReport()
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

                var result = await _reportService.GetParcelsReportAsync(
                    user.SocietyId.Value);

                return Ok(new
                {
                    success = true,
                    message = "Parcel report retrieved successfully.",
                    data = result
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
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An unexpected error occurred."
                });
            }
        }

        // SocietyAdmin: Get Complaint Report
        [HttpGet("reports/complaints")]
        public async Task<IActionResult> GetComplaintsReport()
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

                var result = await _reportService.GetComplaintsReportAsync(
                    user.SocietyId.Value);

                return Ok(new
                {
                    success = true,
                    message = "Complaint report retrieved successfully.",
                    data = result
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
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An unexpected error occurred."
                });
            }
        }

        // SocietyAdmin: Get Amenity Access Report
        [HttpGet("reports/amenity-access")]
        public async Task<IActionResult> GetAmenityAccessReport()
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

                var result = await _reportService.GetAmenityAccessReportAsync(
                    user.SocietyId.Value);

                return Ok(new
                {
                    success = true,
                    message = "Amenity access report retrieved successfully.",
                    data = result
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
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An unexpected error occurred."
                });
            }
        }
    }
}
