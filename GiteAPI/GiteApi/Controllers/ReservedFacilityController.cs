using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using GiteApi.Data;
using GiteApi.Models;

namespace GiteApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservedFacilityController : ControllerBase
    {
        private readonly DatabaseHelper _db;

        public ReservedFacilityController(DatabaseHelper db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReservedFacility>>> GetReservedFacilities()
        {
            var sql = @"SELECT rf.*, f.FacilityName, r.ReservationType, r.StartDate, r.EndDate, r.ReservationID
                        FROM ReservedFacility rf
                        LEFT JOIN Facility f ON rf.FacilityId = f.FacilityId
                        LEFT JOIN Reservation r ON rf.ReservationId = r.ReservationID
                        ORDER BY rf.ReservationId, rf.FacilityId";

            var dt = await _db.ExecuteQueryAsync(sql);

            var reservedFacilities = new List<ReservedFacility>();
            var reservationIds = new List<int>();

            foreach (DataRow row in dt.Rows)
            {
                var reservedFacility = new ReservedFacility
                {
                    FacilityId = Convert.ToInt32(row["FacilityId"]),
                    ReservationId = Convert.ToInt32(row["ReservationId"])
                };

                // Set related properties if they exist in the result
                if (row.Table.Columns.Contains("FacilityName") && !row.IsNull("FacilityName"))
                {
                    reservedFacility.Facility = new Facility
                    {
                        FacilityID = Convert.ToInt32(row["FacilityId"]),
                        FacilityName = row["FacilityName"].ToString()!
                    };
                }

                if (row.Table.Columns.Contains("ReservationType") && !row.IsNull("ReservationType"))
                {
                    var reservationId = Convert.ToInt32(row["ReservationID"]);
                    reservedFacility.Reservation = new Reservation
                    {
                        ReservationID = reservationId,
                        ReservationType = row["ReservationType"].ToString()!,
                        StartDate = Convert.ToDateTime(row["StartDate"]),
                        EndDate = Convert.ToDateTime(row["EndDate"])
                    };
                    
                    reservationIds.Add(reservationId);
                }

                reservedFacilities.Add(reservedFacility);
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
                foreach (var reservedFacility in reservedFacilities)
                {
                    if (reservedFacility.Reservation != null && 
                        invoicesByReservationId.ContainsKey(reservedFacility.Reservation.ReservationID))
                    {
                        reservedFacility.Reservation.Invoice = invoicesByReservationId[reservedFacility.Reservation.ReservationID];
                    }
                }
            }

            return Ok(reservedFacilities);
        }

        [HttpGet("{reservationId}/{facilityId}")]
        public async Task<ActionResult<ReservedFacility>> GetReservedFacility(
            int reservationId,
            int facilityId)
        {
            var sql = @"SELECT rf.*, f.FacilityName, r.ReservationType, r.StartDate, r.EndDate, r.ReservationID
                        FROM ReservedFacility rf
                        LEFT JOIN Facility f ON rf.FacilityId = f.FacilityId
                        LEFT JOIN Reservation r ON rf.ReservationId = r.ReservationID
                        WHERE rf.ReservationId = @reservationId AND rf.FacilityId = @facilityId";

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@reservationId", reservationId),
                new SqlParameter("@facilityId", facilityId)
            };

            var dt = await _db.ExecuteQueryAsync(sql, parameters);

            if (dt.Rows.Count == 0) return NotFound();

            var row = dt.Rows[0];
            var reservedFacility = new ReservedFacility
            {
                FacilityId = Convert.ToInt32(row["FacilityId"]),
                ReservationId = Convert.ToInt32(row["ReservationId"])
            };

            // Set related properties if they exist in the result
            if (!row.IsNull("FacilityName"))
            {
                reservedFacility.Facility = new Facility
                {
                    FacilityID = Convert.ToInt32(row["FacilityId"]),
                    FacilityName = row["FacilityName"].ToString()!
                };
            }

            if (!row.IsNull("ReservationType"))
            {
                var reservationIdFromRow = Convert.ToInt32(row["ReservationID"]);
                reservedFacility.Reservation = new Reservation
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

                reservedFacility.Reservation.Invoice = invoices;
            }

            return Ok(reservedFacility);
        }

        [HttpPost]
        public async Task<ActionResult<ReservedFacility>> PostReservedFacility(
            ReservedFacility reservedFacility)
        {
            // Check if already exists
            var checkSql = @"SELECT COUNT(*) FROM ReservedFacility 
                            WHERE ReservationId = @reservationId AND FacilityId = @facilityId";
            var checkParams = new List<SqlParameter>
            {
                new SqlParameter("@reservationId", reservedFacility.ReservationId),
                new SqlParameter("@facilityId", reservedFacility.FacilityId)
            };

            var count = await _db.ExecuteScalarAsync(checkSql, checkParams);
            if (Convert.ToInt32(count) > 0)
            {
                return Conflict("This facility is already reserved for this reservation.");
            }

            var sql = @"INSERT INTO ReservedFacility (ReservationId, FacilityId)
                        VALUES (@ReservationId, @FacilityId)";

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@ReservationId", reservedFacility.ReservationId),
                new SqlParameter("@FacilityId", reservedFacility.FacilityId)
            };

            var affectedRows = await _db.ExecuteNonQueryAsync(sql, parameters);

            if (affectedRows == 0) return BadRequest();

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

        [HttpDelete("{reservationId}/{facilityId}")]
        public async Task<IActionResult> DeleteReservedFacility(
            int reservationId,
            int facilityId)
        {
            // Check if exists
            var checkSql = @"SELECT COUNT(*) FROM ReservedFacility 
                            WHERE ReservationId = @reservationId AND FacilityId = @facilityId";
            var checkParams = new List<SqlParameter>
            {
                new SqlParameter("@reservationId", reservationId),
                new SqlParameter("@facilityId", facilityId)
            };

            var count = await _db.ExecuteScalarAsync(checkSql, checkParams);
            if (Convert.ToInt32(count) == 0) return NotFound();

            // Delete
            var sql = @"DELETE FROM ReservedFacility 
                       WHERE ReservationId = @reservationId AND FacilityId = @facilityId";
            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@reservationId", reservationId),
                new SqlParameter("@facilityId", facilityId)
            };

            await _db.ExecuteNonQueryAsync(sql, parameters);

            return NoContent();
        }
    }
}