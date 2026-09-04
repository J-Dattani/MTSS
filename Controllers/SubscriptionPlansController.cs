using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MTSS.DTOs.SubscriptionPlan;
using MTSS.Services;

namespace MTSS.Controllers
{
    [ApiController]
    [Route("api/superadmin/subscription-plans")]

    // Only authenticated users with the SuperAdmin role can access this controller.
    [Authorize(Roles = "SuperAdmin")]
    public class SubscriptionPlansController : ControllerBase
    {
        private readonly SubscriptionPlanService _service;

        public SubscriptionPlansController(SubscriptionPlanService service)
        {
            _service = service;
        }

        // GET: api/superadmin/subscription-plans
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

        // POST: api/superadmin/subscription-plans
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

        // GET: api/superadmin/subscription-plans/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var plan = await _service.GetByIdAsync(id);

            if (plan == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Subscription plan not found."
                });
            }

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

            return Ok(new
            {
                success = true,
                message = "Subscription plan retrieved successfully.",
                data = response
            });
        }

        // PUT: api/superadmin/subscription-plans/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            int id,
            UpdateSubscriptionPlanRequest request)
        {
            try
            {
                var plan = await _service.UpdateAsync(id, request);

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

                return Ok(new
                {
                    success = true,
                    message = "Subscription plan updated successfully.",
                    data = response
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

        // PATCH: api/superadmin/subscription-plans/{id}/status
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(
            int id,
            UpdateSubscriptionPlanStatusRequest request)
        {
            try
            {
                var plan = await _service.SetActiveStatusAsync(
                    id,
                    request.IsActive);

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

                return Ok(new
                {
                    success = true,
                    message = "Subscription plan status updated successfully.",
                    data = response
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

        // DELETE: api/superadmin/subscription-plans/{id}
        // Performs a soft delete by setting IsActive = false.
        // DELETE: api/superadmin/subscription-plans/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _service.DeleteAsync(id);

                return Ok(new
                {
                    success = true,
                    message = "Subscription plan deleted successfully.",
                    data = new
                    {
                        subscriptionPlanId = id
                    }
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
        }
    }
}