using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MTSS.DTOs.Society;
using MTSS.Services;

namespace MTSS.Controllers
{
    [ApiController]
    [Route("api/superadmin/societies")]
    [Authorize(Roles = "SuperAdmin")]
    public class SocietiesController : ControllerBase
    {
        private readonly SocietyService _service;

        public SocietiesController(SocietyService service)
        {
            _service = service;
        }

        // GET: api/superadmin/societies
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var societies = await _service.GetAllAsync();

            var response = societies.Select(MapResponse).ToList();

            return Ok(new
            {
                success = true,
                message = "Societies retrieved successfully.",
                data = response
            });
        }

        // POST: api/superadmin/societies
        [HttpPost]
        public async Task<IActionResult> Create(
            CreateSocietyRequest request)
        {
            try
            {
                var society = await _service.CreateAsync(request);

                return StatusCode(201, new
                {
                    success = true,
                    message = "Society created successfully.",
                    data = MapResponse(society)
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

        // GET: api/superadmin/societies/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var society = await _service.GetByIdAsync(id);

            if (society == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Society not found."
                });
            }

            return Ok(new
            {
                success = true,
                message = "Society retrieved successfully.",
                data = MapResponse(society)
            });
        }

        // PUT: api/superadmin/societies/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            int id,
            UpdateSocietyRequest request)
        {
            try
            {
                var society = await _service.UpdateAsync(id, request);

                return Ok(new
                {
                    success = true,
                    message = "Society updated successfully.",
                    data = MapResponse(society)
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

        // PATCH: api/superadmin/societies/{id}/status
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(
            int id,
            UpdateSocietyStatusRequest request)
        {
            try
            {
                var society = await _service.SetActiveStatusAsync(
                    id,
                    request.IsActive);

                return Ok(new
                {
                    success = true,
                    message = "Society status updated successfully.",
                    data = MapResponse(society)
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

        private static SocietyResponse MapResponse(
            Models.Society society)
        {
            return new SocietyResponse
            {
                SocietyId = society.SocietyId,
                Name = society.Name,
                Address = society.Address,
                City = society.City,
                State = society.State,
                Pincode = society.Pincode,
                IsActive = society.IsActive,
                CreatedAt = society.CreatedAt
            };
        }
    }
}