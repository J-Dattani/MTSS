using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MTSS.DTOs.SubscriptionPlan;
using MTSS.Services;

namespace MTSS.Controllers
{
    [ApiController]
    [Route("api/superadmin/subscription-plans")]

    //here only authenticated users with the SuperAdmin role can access this controller.
    [Authorize(Roles = "SuperAdmin")]
    public class SubscriptionPlansController : ControllerBase
    {
        private readonly SubscriptionPlanService _service;

        public SubscriptionPlansController(SubscriptionPlanService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var plans = await _service.GetAllAsync();

            var response = plans.Select(plan => new SubscriptionPlanResponse
            {
                SubscriptionPlanId = plan.SubscriptionPlanId,
                Name = plan.Name,
                MaxWings = plan.MaxWings,
                MaxFloorsPerWing = plan.MaxFloorsPerWing,
                MaxFlats = plan.MaxFlats,
                MaxAmenities = plan.MaxAmenities,
                MaxSocietyAdmins = plan.MaxSocietyAdmins,
                GuestApprovalEnabled = plan.GuestApprovalEnabled,
                ParcelRegisterEnabled = plan.ParcelRegisterEnabled,
                AmenityPinEnabled = plan.AmenityPinEnabled,
                ComplaintManagementEnabled = plan.ComplaintManagementEnabled,
                ReportsEnabled = plan.ReportsEnabled,
                IsActive = plan.IsActive,
                CreatedAt = plan.CreatedAt
            }).ToList();

            return Ok(new
            {
                success = true,
                message = "Subscription plans retrieved successfully.",
                data = response
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateSubscriptionPlanRequest request)
        {
            try
            {
                var plan = await _service.CreateAsync(request);

                var response = new SubscriptionPlanResponse
                {
                    SubscriptionPlanId = plan.SubscriptionPlanId,
                    Name = plan.Name,
                    MaxWings = plan.MaxWings,
                    MaxFloorsPerWing = plan.MaxFloorsPerWing,
                    MaxFlats = plan.MaxFlats,
                    MaxAmenities = plan.MaxAmenities,
                    MaxSocietyAdmins = plan.MaxSocietyAdmins,
                    GuestApprovalEnabled = plan.GuestApprovalEnabled,
                    ParcelRegisterEnabled = plan.ParcelRegisterEnabled,
                    AmenityPinEnabled = plan.AmenityPinEnabled,
                    ComplaintManagementEnabled = plan.ComplaintManagementEnabled,
                    ReportsEnabled = plan.ReportsEnabled,
                    IsActive = plan.IsActive,
                    CreatedAt = plan.CreatedAt
                };

                return StatusCode(201, new
                {
                    success = true,
                    message = "Subscription plan created successfully.",
                    data = response
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