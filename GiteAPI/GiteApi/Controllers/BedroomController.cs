﻿using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using GiteApi.Data;
using GiteApi.Models;
using System.Collections.Generic;

namespace GiteApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BedroomController : ControllerBase
    {
        private readonly DatabaseHelper _db;

        public BedroomController(DatabaseHelper db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Bedroom>>> GetBedrooms()
        {
            var bedroomsDict = new Dictionary<int, Bedroom>();
            var reservationsDict = new Dictionary<int, Reservation>();

            var reservationSql = @"
                SELECT 
                    b.bedroomID, b.bedroomName, b.capacity, b.description, b.availabilityStatus,
                    r.reservationID, r.userID, r.reservationType, r.bedroomID as res_bedroomID,
                    r.startDate, r.endDate, r.numberOfPersons, r.reservationStatus
                FROM Bedroom b
                LEFT JOIN Reservation r ON b.bedroomID = r.bedroomID
                ORDER BY b.bedroomID, r.startDate";

            var dtReservations = await _db.ExecuteQueryAsync(reservationSql);

            foreach (DataRow row in dtReservations.Rows)
            {
                int bedroomId = Convert.ToInt32(row["bedroomID"]);

                if (!bedroomsDict.ContainsKey(bedroomId))
                {
                    bedroomsDict[bedroomId] = new Bedroom
                    {
                        BedroomID = bedroomId,
                        BedroomName = row["bedroomName"].ToString()!,
                        Capacity = Convert.ToInt32(row["capacity"]),
                        Description = row["description"]?.ToString(),
                        AvailabilityStatus = row["availabilityStatus"]?.ToString() ?? "Available",
                        Reservation = new List<Reservation>()
                    };
                }

                var bedroom = bedroomsDict[bedroomId];

                if (!row.IsNull("reservationID"))
                {
                    int reservationId = Convert.ToInt32(row["reservationID"]);
                    
                    if (!reservationsDict.ContainsKey(reservationId))
                    {
                        var reservation = new Reservation
                        {
                            ReservationID = reservationId,
                            UserID = Convert.ToInt32(row["userID"]),
                            ReservationType = row["reservationType"].ToString()!,
                            BedroomID = Convert.ToInt32(row["res_bedroomID"]),
                            StartDate = Convert.ToDateTime(row["startDate"]),
                            EndDate = Convert.ToDateTime(row["endDate"]),
                            NumberOfPersons = Convert.ToInt32(row["numberOfPersons"]),
                            ReservationStatus = row["reservationStatus"].ToString()!,
                            Invoice = new List<Invoice>()
                        };

                        reservationsDict[reservationId] = reservation;
                        bedroom.Reservation!.Add(reservation);
                    }
                    else
                    {
                        if (!bedroom.Reservation!.Any(r => r.ReservationID == reservationId))
                        {
                            bedroom.Reservation!.Add(reservationsDict[reservationId]);
                        }
                    }
                }
            }

            if (reservationsDict.Keys.Any())
            {
                var reservationIds = string.Join(",", reservationsDict.Keys);
                var invoiceSql = $@"
                    SELECT * FROM Invoice 
                    WHERE reservationID IN ({reservationIds})
                    ORDER BY reservationID, issueDate";

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

                foreach (var reservationId in reservationsDict.Keys)
                {
                    if (invoicesByReservationId.ContainsKey(reservationId))
                    {
                        reservationsDict[reservationId].Invoice = invoicesByReservationId[reservationId];
                    }
                }
            }

            return Ok(bedroomsDict.Values.ToList());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Bedroom>> GetBedroom(int id)
        {
            var sql = @"
                SELECT 
                    b.bedroomID, b.bedroomName, b.capacity, b.description, b.availabilityStatus,
                    r.reservationID, r.userID, r.reservationType, r.bedroomID as res_bedroomID,
                    r.startDate, r.endDate, r.numberOfPersons, r.reservationStatus
                FROM Bedroom b
                LEFT JOIN Reservation r ON b.bedroomID = r.bedroomID
                WHERE b.bedroomID = @id
                ORDER BY r.startDate";

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@id", id)
            };

            var dt = await _db.ExecuteQueryAsync(sql, parameters);

            if (dt.Rows.Count == 0) return NotFound();

            Bedroom? bedroom = null;
            var reservationIds = new List<int>();

            foreach (DataRow row in dt.Rows)
            {
                if (bedroom == null)
                {
                    bedroom = new Bedroom
                    {
                        BedroomID = Convert.ToInt32(row["bedroomID"]),
                        BedroomName = row["bedroomName"].ToString()!,
                        Capacity = Convert.ToInt32(row["capacity"]),
                        Description = row["description"]?.ToString(),
                        AvailabilityStatus = row["availabilityStatus"]?.ToString() ?? "Available",
                        Reservation = new List<Reservation>()
                    };
                }

                if (!row.IsNull("reservationID"))
                {
                    int reservationId = Convert.ToInt32(row["reservationID"]);
                    
                    if (!reservationIds.Contains(reservationId))
                    {
                        reservationIds.Add(reservationId);
                        
                        var reservation = new Reservation
                        {
                            ReservationID = reservationId,
                            UserID = Convert.ToInt32(row["userID"]),
                            ReservationType = row["reservationType"].ToString()!,
                            BedroomID = Convert.ToInt32(row["res_bedroomID"]),
                            StartDate = Convert.ToDateTime(row["startDate"]),
                            EndDate = Convert.ToDateTime(row["endDate"]),
                            NumberOfPersons = Convert.ToInt32(row["numberOfPersons"]),
                            ReservationStatus = row["reservationStatus"].ToString()!,
                            Invoice = new List<Invoice>()
                        };

                        bedroom.Reservation!.Add(reservation);
                    }
                }
            }

            if (reservationIds.Any())
            {
                var reservationIdList = string.Join(",", reservationIds);
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

                foreach (var reservation in bedroom!.Reservation!)
                {
                    if (invoicesByReservationId.ContainsKey(reservation.ReservationID))
                    {
                        reservation.Invoice = invoicesByReservationId[reservation.ReservationID];
                    }
                }
            }

            return Ok(bedroom);
        }

        [HttpPost]
        public async Task<ActionResult<Bedroom>> PostBedroom(Bedroom bedroom)
        {
            var sql = @"INSERT INTO Bedroom (bedroomName, capacity, description, availabilityStatus)
                        VALUES (@bedroomName, @capacity, @description, @availabilityStatus);
                        SELECT SCOPE_IDENTITY();";

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@bedroomName", bedroom.BedroomName),
                new SqlParameter("@capacity", bedroom.Capacity),
                new SqlParameter("@description", bedroom.Description ?? (object)DBNull.Value),
                new SqlParameter("@availabilityStatus", bedroom.AvailabilityStatus ?? "Available")
            };

            var newId = await _db.ExecuteScalarAsync(sql, parameters);
            bedroom.BedroomID = Convert.ToInt32(newId);

            return CreatedAtAction(nameof(GetBedroom), new { id = bedroom.BedroomID }, bedroom);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutBedroom(int id, Bedroom bedroom)
        {
            if (id != bedroom.BedroomID) return BadRequest();

            var sql = @"UPDATE Bedroom SET
                        bedroomName = @bedroomName,
                        capacity = @capacity,
                        description = @description,
                        availabilityStatus = @availabilityStatus
                        WHERE bedroomID = @id";

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@bedroomName", bedroom.BedroomName),
                new SqlParameter("@capacity", bedroom.Capacity),
                new SqlParameter("@description", bedroom.Description ?? (object)DBNull.Value),
                new SqlParameter("@availabilityStatus", bedroom.AvailabilityStatus ?? "Available"),
                new SqlParameter("@id", id)
            };

            var affectedRows = await _db.ExecuteNonQueryAsync(sql, parameters);

            if (affectedRows == 0) return NotFound();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBedroom(int id)
        {
            var sql = "DELETE FROM Bedroom WHERE bedroomID = @id";
            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@id", id)
            };

            var affectedRows = await _db.ExecuteNonQueryAsync(sql, parameters);

            if (affectedRows == 0) return NotFound();

            return NoContent();
        }

        [HttpGet("available")]
        public async Task<ActionResult<IEnumerable<Bedroom>>> GetAvailableBedrooms(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int? capacity = null)
        {
            var sql = @"SELECT * FROM Bedroom 
                        WHERE availabilityStatus = 'Available'";

            var parameters = new List<SqlParameter>();

            if (capacity.HasValue)
            {
                sql += " AND capacity >= @capacity";
                parameters.Add(new SqlParameter("@capacity", capacity.Value));
            }

            // Check voor beschikbaarheid in bepaalde periode
            if (startDate.HasValue && endDate.HasValue)
            {
                sql = $@"
                    SELECT DISTINCT b.*
                    FROM Bedroom b
                    WHERE b.availabilityStatus = 'Available'
                    {(capacity.HasValue ? " AND b.capacity >= @capacity" : "")}
                    AND b.bedroomID NOT IN (
                        SELECT r.bedroomID 
                        FROM Reservation r 
                        WHERE r.reservationStatus IN ('Confirmed', 'Pending')
                        AND (
                            (r.startDate <= @endDate AND r.endDate >= @startDate) OR
                            (r.startDate BETWEEN @startDate AND @endDate) OR
                            (r.endDate BETWEEN @startDate AND @endDate)
                        )
                    )";

                parameters.Add(new SqlParameter("@startDate", startDate.Value));
                parameters.Add(new SqlParameter("@endDate", endDate.Value));
            }

            sql += " ORDER BY bedroomName";

            var dt = await _db.ExecuteQueryAsync(sql, parameters);

            var bedrooms = new List<Bedroom>();
            foreach (DataRow row in dt.Rows)
            {
                bedrooms.Add(new Bedroom
                {
                    BedroomID = Convert.ToInt32(row["bedroomID"]),
                    BedroomName = row["bedroomName"].ToString()!,
                    Capacity = Convert.ToInt32(row["capacity"]),
                    Description = row["description"]?.ToString(),
                    AvailabilityStatus = row["availabilityStatus"]?.ToString() ?? "Available"
                });
            }

            return Ok(bedrooms);
        }
    }
}