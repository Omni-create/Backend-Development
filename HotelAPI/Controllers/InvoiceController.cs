// // api/InvoiceController.cs
// [ApiController]
// [Route("api/[controller]")]
// public class InvoiceController : ControllerBase
// {
//     private readonly DBConnect _context;

//     public InvoiceController(DBConnect context)
//     {
//         _context = context;
//     }

//     [HttpGet]
//     public async Task<ActionResult<IEnumerable<Invoice>>> GetInvoices()
//     {
//         return await _context.Invoices
//             .Include(i => i.Reservation)
//             .Include(i => i.PaymentInfo)
//             .ToListAsync();
//     }

//     [HttpGet("{id}")]
//     public async Task<ActionResult<Invoice>> GetInvoice(int id)
//     {
//         var invoice = await _context.Invoices
//             .Include(i => i.Reservation)
//             .Include(i => i.PaymentInfo)
//             .FirstOrDefaultAsync(i => i.InvoiceId == id);

//         if (invoice == null) return NotFound();
//         return invoice;
//     }

//     [HttpPost]
//     public async Task<ActionResult<Invoice>> PostInvoice(Invoice invoice)
//     {
//         // Valideer of reservation bestaat
//         if (!await _context.Reservations.AnyAsync(r => r.ReservationId == invoice.ReservationId))
//             return BadRequest("Reservation does not exist");

//         _context.Invoices.Add(invoice);
//         await _context.SaveChangesAsync();

//         return CreatedAtAction(nameof(GetInvoice), new { id = invoice.InvoiceId }, invoice);
//     }


// }