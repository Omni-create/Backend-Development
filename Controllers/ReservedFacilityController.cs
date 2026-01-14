using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelApi.Data;
using HotelApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservedFacilityController : ControllerBase
    {
        private readonly DBConnect _context;

        public ReservedFacilityController(DBConnect context)
        {
            _context = context;
        }

        // GET: api/reservedfacility
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReservedFacility>>> GetReservedFacilities()
        {
            return await _context.ReservedFacilities
                .Include(rf => rf.Reservation)
                .Include(rf => rf.Facility)
                .ToListAsync();
        }

        // GET: api/reservedfacility/{reservationId}/{facilityId}
        [HttpGet("{reservationId}/{facilityId}")]
        public async Task<ActionResult<ReservedFacility>> GetReservedFacility(
            int reservationId,
            int facilityId)
        {
            var reservedFacility = await _context.ReservedFacilities
                .Include(rf => rf.Reservation)
                .Include(rf => rf.Facility)
                .FirstOrDefaultAsync(rf =>
                    rf.ReservationId == reservationId &&
                    rf.FacilityId == facilityId);

            if (reservedFacility == null)
                return NotFound();

            return reservedFacility;
        }

        // POST: api/reservedfacility
        [HttpPost]
        public async Task<ActionResult<ReservedFacility>> PostReservedFacility(
            ReservedFacility reservedFacility)
        {
            _context.ReservedFacilities.Add(reservedFacility);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetReservedFacility),
                new
                {
                    reservationId = reservedFacility.ReservationId,
                    facilityId = reservedFacility.FacilityId
                },
                reservedFacility
            );
        }

        // DELETE: api/reservedfacility/{reservationId}/{facilityId}
        [HttpDelete("{reservationId}/{facilityId}")]
        public async Task<IActionResult> DeleteReservedFacility(
            int reservationId,
            int facilityId)
        {
            var reservedFacility = await _context.ReservedFacilities
                .FindAsync(reservationId, facilityId);

            if (reservedFacility == null)
                return NotFound();

            _context.ReservedFacilities.Remove(reservedFacility);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
