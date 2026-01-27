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
    public class PaymentInfoController : ControllerBase
    {
        private readonly DBConnect _context;

        public PaymentInfoController(DBConnect context)
        {
            _context = context;
        }

        // GET: api/paymentinfo
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PaymentInfo>>> GetPaymentInfos()
        {
            return await _context.PaymentInfos
    .Include(p => p.User)
    .Include(p => p.Invoices)
    .ToListAsync();
        }

        // GET: api/paymentinfo/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentInfo>> GetPaymentInfo(int id)
        {
            var paymentInfo = await _context.PaymentInfos
    .Include(p => p.User)
    .Include(p => p.Invoices)
    .FirstOrDefaultAsync(p => p.PaymentInfoId == id);

            if (paymentInfo == null)
                return NotFound();

            return paymentInfo;
        }

        // POST: api/paymentinfo
        [HttpPost]
        public async Task<ActionResult<PaymentInfo>> PostPaymentInfo(PaymentInfo paymentInfo)
        {
            _context.PaymentInfos.Add(paymentInfo);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetPaymentInfo),
                new { id = paymentInfo.PaymentInfoId },
                paymentInfo
            );
        }

        // PUT: api/paymentinfo/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPaymentInfo(int id, PaymentInfo paymentInfo)
        {
            if (id != paymentInfo.PaymentInfoId)
                return BadRequest();

            _context.Entry(paymentInfo).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PaymentInfoExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/paymentinfo/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePaymentInfo(int id)
        {
            var paymentInfo = await _context.PaymentInfos.FindAsync(id);

            if (paymentInfo == null)
                return NotFound();

            _context.PaymentInfos.Remove(paymentInfo);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PaymentInfoExists(int id) =>
            _context.PaymentInfos.Any(p => p.PaymentInfoId == id);
    }
}
