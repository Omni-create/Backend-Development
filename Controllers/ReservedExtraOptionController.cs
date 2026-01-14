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
    public class ReservedExtraOptionController : ControllerBase
    {
        private readonly DBConnect _context;

        public ReservedExtraOptionController(DBConnect context)
        {
            _context = context;
        }

        // GET: api/reservedextraoption
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReservedExtraOption>>> GetReservedExtraOptions()
        {
            return await _context.ReservedExtraOptions
                .Include(reo => reo.Reservation)
                .Include(reo => reo.ExtraOption)
                .ToListAsync();
        }

        // GET: api/reservedextraoption/{reservationId}/{extraOptionId}
        [HttpGet("{reservationId}/{extraOptionId}")]
        public async Task<ActionResult<ReservedExtraOption>> GetReservedExtraOption(
            int reservationId,
            int extraOptionId)
        {
            var reservedExtraOption = await _context.ReservedExtraOptions
                .Include(reo => reo.Reservation)
                .Include(reo => reo.ExtraOption)
                .FirstOrDefaultAsync(reo =>
                    reo.ReservationId == reservationId &&
                    reo.ExtraOptionId == extraOptionId);

            if (reservedExtraOption == null)
                return NotFound();

            return reservedExtraOption;
        }

        // POST: api/reservedextraoption
        [HttpPost]
        public async Task<ActionResult<ReservedExtraOption>> PostReservedExtraOption(
            ReservedExtraOption reservedExtraOption)
        {
            _context.ReservedExtraOptions.Add(reservedExtraOption);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetReservedExtraOption),
                new
                {
                    reservationId = reservedExtraOption.ReservationId,
                    extraOptionId = reservedExtraOption.ExtraOptionId
                },
                reservedExtraOption
            );
        }

        // DELETE: api/reservedextraoption/{reservationId}/{extraOptionId}
        [HttpDelete("{reservationId}/{extraOptionId}")]
        public async Task<IActionResult> DeleteReservedExtraOption(
            int reservationId,
            int extraOptionId)
        {
            var reservedExtraOption = await _context.ReservedExtraOptions
                .FindAsync(reservationId, extraOptionId);

            if (reservedExtraOption == null)
                return NotFound();

            _context.ReservedExtraOptions.Remove(reservedExtraOption);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
