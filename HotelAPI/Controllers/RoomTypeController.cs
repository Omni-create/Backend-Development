using HotelApi.Data;
using HotelApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoomTypesController : ControllerBase
    {
        private readonly DBConnect _context;

        public RoomTypesController(DBConnect context)
        {
            _context = context;
        }

        // GET: api/roomtypes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoomType>>> GetRoomTypes()
        {
            return await _context.RoomTypes.Include(rt => rt.Rooms).ToListAsync();
        }

        // GET: api/roomtypes/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<RoomType>> GetRoomType(int id)
        {
            var roomType = await _context.RoomTypes
    .Include(rt => rt.Rooms)
    .FirstOrDefaultAsync(rt => rt.RoomTypeId == id);

            if (roomType == null)
                return NotFound();

            return roomType;
        }

        // POST: api/roomtypes
        [HttpPost]
        public async Task<ActionResult<RoomType>> PostRoomType(RoomType roomType)
        {
            _context.RoomTypes.Add(roomType);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetRoomType),
                new { id = roomType.RoomTypeId },
                roomType
            );
        }

        // PUT: api/roomtypes/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRoomType(int id, RoomType roomType)
        {
            if (id != roomType.RoomTypeId)
                return BadRequest();

            _context.Entry(roomType).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RoomTypeExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/roomtypes/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoomType(int id)
        {
            var roomType = await _context.RoomTypes.FindAsync(id);

            if (roomType == null)
                return NotFound();

            _context.RoomTypes.Remove(roomType);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool RoomTypeExists(int id) =>
            _context.RoomTypes.Any(rt => rt.RoomTypeId == id);
    }
}
