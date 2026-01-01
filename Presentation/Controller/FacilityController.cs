using Microsoft.AspNetCore.Mvc;
using Backend_Dev.Models;
using Presentation.Transfer;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Presentation.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class FacilityController : ControllerBase
    {
        private readonly IFacilityService _facilityService;

        public FacilityController(IFacilityService facilityService)
        {
            _facilityService = facilityService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FacilityDto>>> GetFacilities()
        {
            var facilities = await _facilityService.GetAllFacilitiesAsync();
            return Ok(facilities);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FacilityDto>> GetFacility(int id)
        {
            var facility = await _facilityService.GetFacilityByIdAsync(id);
            if (facility == null)
                return NotFound();
            return Ok(facility);
        }

        [HttpPost]
        public async Task<ActionResult<FacilityDto>> CreateFacility(CreateFacilityDto dto)
        {
            var facility = await _facilityService.CreateFacilityAsync(dto);
            return CreatedAtAction(nameof(GetFacility), new { id = facility.FacilityId }, facility);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFacility(int id, CreateFacilityDto dto)
        {
            await _facilityService.UpdateFacilityAsync(id, dto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFacility(int id)
        {
            await _facilityService.DeleteFacilityAsync(id);
            return NoContent();
        }
    }
}
