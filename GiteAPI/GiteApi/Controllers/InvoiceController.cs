using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using GiteApi.Data;
using GiteApi.Models;
using System.Collections.Generic;

namespace GiteApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoiceController : ControllerBase
    {
        private readonly DatabaseHelper _db;

        public InvoiceController(DatabaseHelper db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Invoice>>> GetInvoices()
        {
            var sql = @"SELECT * FROM Invoice ORDER BY invoiceID";

            var dt = await _db.ExecuteQueryAsync(sql);

            var invoices = new List<Invoice>();
            foreach (DataRow row in dt.Rows)
            {
                var invoice = new Invoice
                {
                    InvoiceID = Convert.ToInt32(row["invoiceID"]),
                    ReservationID = Convert.ToInt32(row["reservationID"]),
                    PaymentInfoID = row["paymentInfoID"] != DBNull.Value ? Convert.ToInt32(row["paymentInfoID"]) : (int?)null,
                    Description = row["description"]?.ToString(),
                    TotalCost = Convert.ToDecimal(row["totalCost"]),
                    PaymentStatus = row["paymentStatus"].ToString()!,
                    IssueDate = Convert.ToDateTime(row["issueDate"])
                };

                // Add reservation and user info if needed (for display purposes)
                if (!row.IsNull("reservationType"))
                {
                    // You could create a DTO to include this info if needed
                }

                invoices.Add(invoice);
            }

            return Ok(invoices);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Invoice>> GetInvoice(int id)
        {
            var sql = @"SELECT * FROM Invoice WHERE invoiceID = @id";

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@id", id)
            };

            var dt = await _db.ExecuteQueryAsync(sql, parameters);

            if (dt.Rows.Count == 0) return NotFound();

            var row = dt.Rows[0];
            var invoice = new Invoice
            {
                InvoiceID = Convert.ToInt32(row["invoiceID"]),
                ReservationID = Convert.ToInt32(row["reservationID"]),
                PaymentInfoID = row["paymentInfoID"] != DBNull.Value ? Convert.ToInt32(row["paymentInfoID"]) : (int?)null,
                Description = row["description"]?.ToString(),
                TotalCost = Convert.ToDecimal(row["totalCost"]),
                PaymentStatus = row["paymentStatus"].ToString()!,
                IssueDate = Convert.ToDateTime(row["issueDate"])
            };

            return Ok(invoice);
        }

        [HttpPost]
        public async Task<ActionResult<Invoice>> PostInvoice(Invoice invoice)
        {
            var sql = @"INSERT INTO Invoice 
                        (reservationID, paymentInfoID, description, totalCost, paymentStatus, issueDate) 
                        VALUES 
                        (@reservationID, @paymentInfoID, @description, @totalCost, @paymentStatus, @issueDate);
                        SELECT SCOPE_IDENTITY();";

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@reservationID", invoice.ReservationID),
                new SqlParameter("@paymentInfoID", invoice.PaymentInfoID ?? (object)DBNull.Value),
                new SqlParameter("@description", invoice.Description ?? (object)DBNull.Value),
                new SqlParameter("@totalCost", invoice.TotalCost),
                new SqlParameter("@paymentStatus", invoice.PaymentStatus),
                new SqlParameter("@issueDate", invoice.IssueDate)
            };

            var newId = await _db.ExecuteScalarAsync(sql, parameters);
            invoice.InvoiceID = Convert.ToInt32(newId);

            return CreatedAtAction(nameof(GetInvoice), new { id = invoice.InvoiceID }, invoice);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutInvoice(int id, Invoice invoice)
        {
            if (id != invoice.InvoiceID) return BadRequest();

            var sql = @"UPDATE Invoice SET
                        reservationID = @reservationID,
                        paymentInfoID = @paymentInfoID,
                        description = @description,
                        totalCost = @totalCost,
                        paymentStatus = @paymentStatus,
                        issueDate = @issueDate
                        WHERE invoiceID = @id";

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@reservationID", invoice.ReservationID),
                new SqlParameter("@paymentInfoID", invoice.PaymentInfoID ?? (object)DBNull.Value),
                new SqlParameter("@description", invoice.Description ?? (object)DBNull.Value),
                new SqlParameter("@totalCost", invoice.TotalCost),
                new SqlParameter("@paymentStatus", invoice.PaymentStatus),
                new SqlParameter("@issueDate", invoice.IssueDate),
                new SqlParameter("@id", id)
            };

            var affectedRows = await _db.ExecuteNonQueryAsync(sql, parameters);

            if (affectedRows == 0) return NotFound();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInvoice(int id)
        {
            var sql = "DELETE FROM Invoice WHERE invoiceID = @id";
            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@id", id)
            };

            var affectedRows = await _db.ExecuteNonQueryAsync(sql, parameters);

            if (affectedRows == 0) return NotFound();

            return NoContent();
        }
    }

    // Helper classes for patch requests
    public class UpdateStatusRequest
    {
        public string PaymentStatus { get; set; } = null!;
    }

    public class UpdatePaymentInfoRequest
    {
        public int? PaymentInfoID { get; set; }
    }
}