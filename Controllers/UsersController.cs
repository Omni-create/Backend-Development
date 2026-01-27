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
    public class UsersController : ControllerBase
    {
        private readonly DBConnect _context;

        public UsersController(DBConnect context)
        {
            _context = context;
        }

         [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers(
        [FromQuery] string? username = null,
        [FromQuery] string? email = null,
        [FromQuery] UserRole? role = null,
        [FromQuery] string? firstName = null,
        [FromQuery] string? lastName = null)
    {
        var query = _context.Users.AsQueryable();

        // Filter op username
        if (!string.IsNullOrEmpty(username))
        {
            query = query.Where(u => u.Username.Contains(username));
        }

        // Filter op email
        if (!string.IsNullOrEmpty(email))
        {
            query = query.Where(u => u.Email.Contains(email));
        }

        // Filter op role
        if (role.HasValue)
        {
            query = query.Where(u => u.UserRole == role.Value);
        }

        // Filter op firstName
        if (!string.IsNullOrEmpty(firstName))
        {
            query = query.Where(u => u.FirstName.Contains(firstName));
        }

        // Filter op lastName
        if (!string.IsNullOrEmpty(lastName))
        {
            query = query.Where(u => u.LastName.Contains(lastName));
        }

        return await query.Include(u => u.PaymentInfos)
            .Include(u => u.Reservations)
            .ToListAsync();
    }   

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users
    .Include(u => u.PaymentInfos)
    .Include(u => u.Reservations)
    .FirstOrDefaultAsync(u => u.UserId == id);
            if (user == null) return NotFound();

            return user;
        }

        // POST: api/users
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.UserId }, user);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, User user)
        {
            if (id != user.UserId)
            {
                return BadRequest();
            }

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null) return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExists(int id) =>
            _context.Users.Any(u => u.UserId == id);
    }
}
