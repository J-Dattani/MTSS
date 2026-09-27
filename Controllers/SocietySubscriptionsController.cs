using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MTSS.DTOs.SocietySubscription;
using MTSS.Services;

namespace MTSS.Controllers
{
    [ApiController]
    [Route("api/superadmin/societies/{societyId}/subscription")]
    [Authorize(Roles = "SuperAdmin")]
    public class SocietySubscriptionsController : ControllerBase
    {
        private readonly SocietySubscriptionService _service;

        public SocietySubscriptionsController(
            SocietySubscriptionService service)
        {
            _service = service;
        }

        // GET:
        // api/superadmin/societies/{societyId}/subscription
        [HttpGet]
        public async Task<IActionResult> Get(int societyId)
        {
            var subscription =
                await _service.GetBySocietyIdAsync(societyId);

            if (subscription == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Society subscription not found."
                });
            }

            return Ok(new
            {
                success = true,
                message = "Society subscription retrieved successfully.",
                data = MapResponse(subscription)
            });
        }

        // POST:
        // api/superadmin/societies/{societyId}/subscription
        [HttpPost]
        public async Task<IActionResult> Assign(
            int societyId,
            AssignSocietySubscriptionRequest request)
        {
            try
            {
                var subscription =
                    await _service.AssignAsync(societyId, request);

                return StatusCode(201, new
                {
                    success = true,
                    message = "Society subscription assigned successfully.",
                    data = MapResponse(subscription)
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

        // PUT:
        // api/superadmin/societies/{societyId}/subscription
        [HttpPut]
        public async Task<IActionResult> Update(
            int societyId,
            UpdateSocietySubscriptionRequest request)
        {
            try
            {
                var subscription =
                    await _service.UpdateAsync(societyId, request);

                return Ok(new
                {
                    success = true,
                    message = "Society subscription updated successfully.",
                    data = MapResponse(subscription)
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

        // PATCH:
        // api/superadmin/societies/{societyId}/subscription/status
        [HttpPatch("status")]
        public async Task<IActionResult> UpdateStatus(
            int societyId,
            UpdateSocietySubscriptionStatusRequest request)
        {
            try
            {
                var subscription =
                    await _service.UpdateStatusAsync(
                        societyId,
                        request);

                return Ok(new
                {
                    success = true,
                    message = "Society subscription status updated successfully.",
                    data = MapResponse(subscription)
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

        private static SocietySubscriptionResponse MapResponse(
            Models.SocietySubscription subscription)
        {
            return new SocietySubscriptionResponse
            {
                SocietySubscriptionId =
                    subscription.SocietySubscriptionId,

                SocietyId =
                    subscription.SocietyId,

                SubscriptionPlanId =
                    subscription.SubscriptionPlanId,

                Status =
                    subscription.Status,

                StartDate =
                    subscription.StartDate,

                EndDate =
                    subscription.EndDate,

                IsActive =
                    subscription.IsActive,

                CreatedAt =
                    subscription.CreatedAt
            };
        }
    }
}