using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using GiteApi.Data;
using GiteApi.Models;

namespace GiteApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservedExtraOptionController : ControllerBase
    {
        private readonly DatabaseHelper _db;

        public ReservedExtraOptionController(DatabaseHelper db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReservedExtraOption>>> GetReservedExtraOptions()
        {
            var sql = @"SELECT reo.*, eo.OptionName as OptionName, eo.Price as Price, 
                               r.ReservationType, r.StartDate, r.EndDate, r.ReservationID
                        FROM ReservedExtraOption reo
                        LEFT JOIN ExtraOption eo ON reo.ExtraOptionId = eo.ExtraOptionId
                        LEFT JOIN Reservation r ON reo.ReservationId = r.ReservationID
                        ORDER BY reo.ReservationId, reo.ExtraOptionId";

            var dt = await _db.ExecuteQueryAsync(sql);

            var reservedExtraOptions = new List<ReservedExtraOption>();
            var reservationIds = new List<int>();

            foreach (DataRow row in dt.Rows)
            {
                var reservedExtraOption = new ReservedExtraOption
                {
                    ExtraOptionId = Convert.ToInt32(row["ExtraOptionId"]),
                    ReservationId = Convert.ToInt32(row["ReservationId"])
                };

                // Set related properties if they exist in the result
                if (row.Table.Columns.Contains("OptionName") && !row.IsNull("OptionName"))
                {
                    reservedExtraOption.ExtraOption = new ExtraOption
                    {
                        ExtraOptionId = Convert.ToInt32(row["ExtraOptionId"]),
                        OptionName = row["OptionName"].ToString()!,
                        Price = Convert.ToDecimal(row["Price"])
                    };
                }

                if (row.Table.Columns.Contains("ReservationType") && !row.IsNull("ReservationType"))
                {
                    var reservationId = Convert.ToInt32(row["ReservationID"]);
                    reservedExtraOption.Reservation = new Reservation
                    {
                        ReservationID = reservationId,
                        ReservationType = row["ReservationType"].ToString()!,
                        StartDate = Convert.ToDateTime(row["StartDate"]),
                        EndDate = Convert.ToDateTime(row["EndDate"])
                    };
                    
                    reservationIds.Add(reservationId);
                }

                reservedExtraOptions.Add(reservedExtraOption);
            }

            // Add invoices to reservations if they exist
            if (reservationIds.Any())
            {
                var reservationIdList = string.Join(",", reservationIds.Distinct());
                var invoiceSql = $@"
                    SELECT * FROM Invoice 
                    WHERE reservationID IN ({reservationIdList})
                    ORDER BY issueDate";

                var dtInvoices = await _db.ExecuteQueryAsync(invoiceSql);

                var invoicesByReservationId = new Dictionary<int, List<Invoice>>();

                foreach (DataRow row in dtInvoices.Rows)
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

                    if (!invoicesByReservationId.ContainsKey(invoice.ReservationID))
                        invoicesByReservationId[invoice.ReservationID] = new List<Invoice>();

                    invoicesByReservationId[invoice.ReservationID].Add(invoice);
                }

                // Link invoices to reservations
                foreach (var reservedExtraOption in reservedExtraOptions)
                {
                    if (reservedExtraOption.Reservation != null && 
                        invoicesByReservationId.ContainsKey(reservedExtraOption.Reservation.ReservationID))
                    {
                        reservedExtraOption.Reservation.Invoice = invoicesByReservationId[reservedExtraOption.Reservation.ReservationID];
                    }
                }
            }

            return Ok(reservedExtraOptions);
        }

        [HttpGet("{reservationId}/{extraOptionId}")]
        public async Task<ActionResult<ReservedExtraOption>> GetReservedExtraOption(
            int reservationId,
            int extraOptionId)
        {
            var sql = @"SELECT reo.*, eo.OptionName as OptionName, eo.Price as Price,
                               r.ReservationType, r.StartDate, r.EndDate, r.ReservationID
                        FROM ReservedExtraOption reo
                        LEFT JOIN ExtraOption eo ON reo.ExtraOptionId = eo.ExtraOptionId
                        LEFT JOIN Reservation r ON reo.ReservationId = r.ReservationID
                        WHERE reo.ReservationId = @reservationId AND reo.ExtraOptionId = @extraOptionId";

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@reservationId", reservationId),
                new SqlParameter("@extraOptionId", extraOptionId)
            };

            var dt = await _db.ExecuteQueryAsync(sql, parameters);

            if (dt.Rows.Count == 0) return NotFound();

            var row = dt.Rows[0];
            var reservedExtraOption = new ReservedExtraOption
            {
                ExtraOptionId = Convert.ToInt32(row["ExtraOptionId"]),
                ReservationId = Convert.ToInt32(row["ReservationId"])
            };

            // Set related properties if they exist in the result
            if (!row.IsNull("OptionName"))
            {
                reservedExtraOption.ExtraOption = new ExtraOption
                {
                    ExtraOptionId = Convert.ToInt32(row["ExtraOptionId"]),
                    OptionName = row["OptionName"].ToString()!,
                    Price = Convert.ToDecimal(row["Price"])
                };
            }

            if (!row.IsNull("ReservationType"))
            {
                var reservationIdFromRow = Convert.ToInt32(row["ReservationID"]);
                reservedExtraOption.Reservation = new Reservation
                {
                    ReservationID = reservationIdFromRow,
                    ReservationType = row["ReservationType"].ToString()!,
                    StartDate = Convert.ToDateTime(row["StartDate"]),
                    EndDate = Convert.ToDateTime(row["EndDate"])
                };

                // Add invoices to the reservation if they exist
                var invoiceSql = @"SELECT * FROM Invoice WHERE reservationID = @reservationId ORDER BY issueDate";
                var invoiceParams = new List<SqlParameter>
                {
                    new SqlParameter("@reservationId", reservationIdFromRow)
                };

                var dtInvoices = await _db.ExecuteQueryAsync(invoiceSql, invoiceParams);
                var invoices = new List<Invoice>();

                foreach (DataRow invoiceRow in dtInvoices.Rows)
                {
                    var invoice = new Invoice
                    {
                        InvoiceID = Convert.ToInt32(invoiceRow["invoiceID"]),
                        ReservationID = Convert.ToInt32(invoiceRow["reservationID"]),
                        PaymentInfoID = invoiceRow["paymentInfoID"] != DBNull.Value ? Convert.ToInt32(invoiceRow["paymentInfoID"]) : (int?)null,
                        Description = invoiceRow["description"]?.ToString(),
                        TotalCost = Convert.ToDecimal(invoiceRow["totalCost"]),
                        PaymentStatus = invoiceRow["paymentStatus"].ToString()!,
                        IssueDate = Convert.ToDateTime(invoiceRow["issueDate"])
                    };
                    invoices.Add(invoice);
                }

                reservedExtraOption.Reservation.Invoice = invoices;
            }

            return Ok(reservedExtraOption);
        }

        [HttpPost]
        public async Task<ActionResult<ReservedExtraOption>> PostReservedExtraOption(
            ReservedExtraOption reservedExtraOption)
        {
            // Check if already exists
            var checkSql = @"SELECT COUNT(*) FROM ReservedExtraOption 
                            WHERE ReservationId = @reservationId AND ExtraOptionId = @extraOptionId";
            var checkParams = new List<SqlParameter>
            {
                new SqlParameter("@reservationId", reservedExtraOption.ReservationId),
                new SqlParameter("@extraOptionId", reservedExtraOption.ExtraOptionId)
            };

            var count = await _db.ExecuteScalarAsync(checkSql, checkParams);
            if (Convert.ToInt32(count) > 0)
            {
                return Conflict("This extra option is already reserved for this reservation.");
            }

            var sql = @"INSERT INTO ReservedExtraOption (ReservationId, ExtraOptionId)
                        VALUES (@ReservationId, @ExtraOptionId)";

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@ReservationId", reservedExtraOption.ReservationId),
                new SqlParameter("@ExtraOptionId", reservedExtraOption.ExtraOptionId)
            };

            var affectedRows = await _db.ExecuteNonQueryAsync(sql, parameters);

            if (affectedRows == 0) return BadRequest();

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

        [HttpDelete("{reservationId}/{extraOptionId}")]
        public async Task<IActionResult> DeleteReservedExtraOption(
            int reservationId,
            int extraOptionId)
        {
            var sql = @"DELETE FROM ReservedExtraOption 
                       WHERE ReservationId = @reservationId AND ExtraOptionId = @extraOptionId";
            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@reservationId", reservationId),
                new SqlParameter("@extraOptionId", extraOptionId)
            };

            var affectedRows = await _db.ExecuteNonQueryAsync(sql, parameters);

            if (affectedRows == 0) return NotFound();

            return NoContent();
        }
    }
}