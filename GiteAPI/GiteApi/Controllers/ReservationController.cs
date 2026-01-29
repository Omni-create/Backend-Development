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
    public class ReservationController : ControllerBase
    {
        private readonly DatabaseHelper _db;

        public ReservationController(DatabaseHelper db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Reservation>>> GetReservations()
        {
            var reservationsDict = new Dictionary<int, Reservation>();

            var invoiceSql = @"
                SELECT 
                    r.reservationID, r.userID, r.reservationType, r.bedroomID,
                    r.startDate, r.endDate, r.numberOfPersons, r.reservationStatus,
                    i.invoiceID, i.reservationID as inv_reservationID, i.paymentInfoID,
                    i.description, i.totalCost, i.paymentStatus, i.issueDate
                FROM Reservation r
                LEFT JOIN Invoice i ON r.reservationID = i.reservationID
                ORDER BY r.reservationID";

            var dtInvoices = await _db.ExecuteQueryAsync(invoiceSql);

            foreach (DataRow row in dtInvoices.Rows)
            {
                int reservationId = Convert.ToInt32(row["reservationID"]);

                if (!reservationsDict.ContainsKey(reservationId))
                {
                    reservationsDict[reservationId] = new Reservation
                    {
                        ReservationID = reservationId,
                        UserID = Convert.ToInt32(row["userID"]),
                        ReservationType = row["reservationType"].ToString()!,
                        BedroomID = row["bedroomID"] != DBNull.Value ? Convert.ToInt32(row["bedroomID"]) : (int?)null,
                        StartDate = Convert.ToDateTime(row["startDate"]),
                        EndDate = Convert.ToDateTime(row["endDate"]),
                        NumberOfPersons = Convert.ToInt32(row["numberOfPersons"]),
                        ReservationStatus = row["reservationStatus"].ToString()!,
                        Invoice = new List<Invoice>()
                    };
                }

                var reservation = reservationsDict[reservationId];

                if (!row.IsNull("invoiceID"))
                {
                    var invoice = new Invoice
                    {
                        InvoiceID = Convert.ToInt32(row["invoiceID"]),
                        ReservationID = Convert.ToInt32(row["inv_reservationID"]),
                        PaymentInfoID = row["paymentInfoID"] != DBNull.Value ? Convert.ToInt32(row["paymentInfoID"]) : (int?)null,
                        Description = row["description"]?.ToString(),
                        TotalCost = Convert.ToDecimal(row["totalCost"]),
                        PaymentStatus = row["paymentStatus"].ToString()!,
                        IssueDate = Convert.ToDateTime(row["issueDate"])
                    };

                    reservation.Invoice!.Add(invoice);
                }
            }

            return Ok(reservationsDict.Values.ToList());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Reservation>> GetReservation(int id)
        {
            var sql = @"
                SELECT 
                    r.reservationID, r.userID, r.reservationType, r.bedroomID,
                    r.startDate, r.endDate, r.numberOfPersons, r.reservationStatus,
                    i.invoiceID, i.reservationID as inv_reservationID, i.paymentInfoID,
                    i.description, i.totalCost, i.paymentStatus, i.issueDate
                FROM Reservation r
                LEFT JOIN Invoice i ON r.reservationID = i.reservationID
                WHERE r.reservationID = @id
                ORDER BY i.issueDate";

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@id", id)
            };

            var dt = await _db.ExecuteQueryAsync(sql, parameters);

            if (dt.Rows.Count == 0) return NotFound();

            Reservation? reservation = null;

            foreach (DataRow row in dt.Rows)
            {
                if (reservation == null)
                {
                    reservation = new Reservation
                    {
                        ReservationID = Convert.ToInt32(row["reservationID"]),
                        UserID = Convert.ToInt32(row["userID"]),
                        ReservationType = row["reservationType"].ToString()!,
                        BedroomID = row["bedroomID"] != DBNull.Value ? Convert.ToInt32(row["bedroomID"]) : (int?)null,
                        StartDate = Convert.ToDateTime(row["startDate"]),
                        EndDate = Convert.ToDateTime(row["endDate"]),
                        NumberOfPersons = Convert.ToInt32(row["numberOfPersons"]),
                        ReservationStatus = row["reservationStatus"].ToString()!,
                        Invoice = new List<Invoice>()
                    };
                }

                if (!row.IsNull("invoiceID"))
                {
                    var invoice = new Invoice
                    {
                        InvoiceID = Convert.ToInt32(row["invoiceID"]),
                        ReservationID = Convert.ToInt32(row["inv_reservationID"]),
                        PaymentInfoID = row["paymentInfoID"] != DBNull.Value ? Convert.ToInt32(row["paymentInfoID"]) : (int?)null,
                        Description = row["description"]?.ToString(),
                        TotalCost = Convert.ToDecimal(row["totalCost"]),
                        PaymentStatus = row["paymentStatus"].ToString()!,
                        IssueDate = Convert.ToDateTime(row["issueDate"])
                    };

                    reservation.Invoice!.Add(invoice);
                }
            }

            return Ok(reservation);
        }

        [HttpPost]
        public async Task<ActionResult<Reservation>> PostReservation(Reservation reservation)
        {
            var sql = @"INSERT INTO Reservation 
                        (userID, reservationType, bedroomID, startDate, endDate, numberOfPersons, reservationStatus) 
                        VALUES 
                        (@userID, @reservationType, @bedroomID, @startDate, @endDate, @numberOfPersons, @reservationStatus);
                        SELECT SCOPE_IDENTITY();";

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@userID", reservation.UserID),
                new SqlParameter("@reservationType", reservation.ReservationType),
                new SqlParameter("@bedroomID", reservation.BedroomID ?? (object)DBNull.Value),
                new SqlParameter("@startDate", reservation.StartDate),
                new SqlParameter("@endDate", reservation.EndDate),
                new SqlParameter("@numberOfPersons", reservation.NumberOfPersons),
                new SqlParameter("@reservationStatus", reservation.ReservationStatus)
            };

            var newId = await _db.ExecuteScalarAsync(sql, parameters);
            reservation.ReservationID = Convert.ToInt32(newId);

            return CreatedAtAction(nameof(GetReservation), new { id = reservation.ReservationID }, reservation);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutReservation(int id, Reservation reservation)
        {
            if (id != reservation.ReservationID) return BadRequest();

            var sql = @"UPDATE Reservation SET
                        userID = @userID,
                        reservationType = @reservationType,
                        bedroomID = @bedroomID,
                        startDate = @startDate,
                        endDate = @endDate,
                        numberOfPersons = @numberOfPersons,
                        reservationStatus = @reservationStatus
                        WHERE reservationID = @id";

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@userID", reservation.UserID),
                new SqlParameter("@reservationType", reservation.ReservationType),
                new SqlParameter("@bedroomID", reservation.BedroomID ?? (object)DBNull.Value),
                new SqlParameter("@startDate", reservation.StartDate),
                new SqlParameter("@endDate", reservation.EndDate),
                new SqlParameter("@numberOfPersons", reservation.NumberOfPersons),
                new SqlParameter("@reservationStatus", reservation.ReservationStatus),
                new SqlParameter("@id", id)
            };

            var affectedRows = await _db.ExecuteNonQueryAsync(sql, parameters);

            if (affectedRows == 0) return NotFound();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReservation(int id)
        {
            var sql = "DELETE FROM Reservation WHERE reservationID = @id";
            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@id", id)
            };

            var affectedRows = await _db.ExecuteNonQueryAsync(sql, parameters);

            if (affectedRows == 0) return NotFound();

            return NoContent();
        }
    }
}