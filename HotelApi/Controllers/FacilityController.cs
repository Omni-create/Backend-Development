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
    public class FacilityController : ControllerBase
    {
        private readonly HotelDbContext _context;

        public FacilityController(HotelDbContext context)
        {
            _context = context;
        }

        // GET: api/facility
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Facility>>> GetFacilities()
        {
            return await _context.Facilities.ToListAsync();
        }

        // GET: api/facility/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Facility>> GetFacility(int id)
        {
            var facility = await _context.Facilities.FindAsync(id);

            if (facility == null)
                return NotFound();

            return facility;
        }

        // POST: api/facility
        [HttpPost]
        public async Task<ActionResult<Facility>> PostFacility(Facility facility)
        {
            _context.Facilities.Add(facility);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetFacility),
                new { id = facility.FacilityId },
                facility
            );
        }

        // PUT: api/facility/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutFacility(int id, Facility facility)
        {
            if (id != facility.FacilityId)
                return BadRequest();

            _context.Entry(facility).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FacilityExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/facility/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFacility(int id)
        {
            var facility = await _context.Facilities.FindAsync(id);

            if (facility == null)
                return NotFound();

            _context.Facilities.Remove(facility);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool FacilityExists(int id) =>
            _context.Facilities.Any(f => f.FacilityId == id);
    }
}
