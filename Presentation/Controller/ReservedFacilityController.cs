using Microsoft.AspNetCore.Mvc;
using Backend_Dev.Models;
using Presentation.Transfer;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Presentation.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservedFacilityController : ControllerBase
    {
        private readonly IReservedFacilityService _reservedFacilityService;

        public ReservedFacilityController(IReservedFacilityService reservedFacilityService)
        {
            _reservedFacilityService = reservedFacilityService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReservedFacilityDto>>> GetReservedFacilities()
        {
            var reservedFacilities = await _reservedFacilityService.GetAllReservedFacilitiesAsync();
            return Ok(reservedFacilities);
        }

        [HttpGet("{reservationId}/{facilityId}")]
        public async Task<ActionResult<ReservedFacilityDto>> GetReservedFacility(int reservationId, int facilityId)
        {
            var reservedFacility = await _reservedFacilityService.GetReservedFacilityAsync(reservationId, facilityId);
            if (reservedFacility == null)
                return NotFound();
            return Ok(reservedFacility);
        }

        [HttpPost]
        public async Task<ActionResult<ReservedFacilityDto>> CreateReservedFacility(CreateReservedFacilityDto dto)
        {
            var reservedFacility = await _reservedFacilityService.CreateReservedFacilityAsync(dto);
            return CreatedAtAction(nameof(GetReservedFacility), new { reservationId = reservedFacility.ReservationId, facilityId = reservedFacility.FacilityId }, reservedFacility);
        }

        [HttpDelete("{reservationId}/{facilityId}")]
        public async Task<IActionResult> DeleteReservedFacility(int reservationId, int facilityId)
        {
            await _reservedFacilityService.DeleteReservedFacilityAsync(reservationId, facilityId);
            return NoContent();
        }
    }
}
