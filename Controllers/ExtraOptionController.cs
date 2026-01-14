using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelApi.Data;
using HotelApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExtraOptionController : ControllerBase
    {
        private readonly DBConnect _context;

        public ExtraOptionController(DBConnect context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ExtraOption>>> GetExtraOptions()
        {
            return await _context.ExtraOptions.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ExtraOption>> GetExtraOption(int id)
        {
            var extraOption = await _context.ExtraOptions.FindAsync(id);

            if (extraOption == null) return NotFound();

            return extraOption;
        }

        [HttpPost]
        public async Task<ActionResult<ExtraOption>> PostExtraOption(ExtraOption extraOption)
        {
            _context.ExtraOptions.Add(extraOption);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetExtraOption), new { id = extraOption.ExtraOptionId }, extraOption);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutExtraOption(int id, ExtraOption extraOption)
        {
            if (id != extraOption.ExtraOptionId)
            {
                return BadRequest();
            }

            _context.Entry(extraOption).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ExtraOptionExists(id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExtraOption(int id)
        {
            var extraOption = await _context.ExtraOptions.FindAsync(id);

            if (extraOption == null) return NotFound();

            _context.ExtraOptions.Remove(extraOption);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ExtraOptionExists(int id) =>
            _context.ExtraOptions.Any(e => e.ExtraOptionId == id);
    }
}
