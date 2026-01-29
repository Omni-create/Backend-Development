using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using GiteApi.Data;
using GiteApi.Models;
using System.Collections.Generic;  // Add this for ICollection

namespace GiteApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly DatabaseHelper _db;

        public UsersController(DatabaseHelper db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {

            var usersDict = new Dictionary<int, User>();

            var reservationSql = @"
        SELECT 
            u.userID, u.username, u.password, u.createdDate, u.userRole, 
            u.firstName, u.lastName, u.email, u.phone,
            r.reservationID, r.userID as res_userID, r.reservationType, 
            r.bedroomID, r.startDate, r.endDate, r.numberOfPersons, r.reservationStatus
        FROM Users u
        LEFT JOIN Reservation r ON u.userID = r.userID
        ORDER BY u.userID";

            var dtReservations = await _db.ExecuteQueryAsync(reservationSql);

            foreach (DataRow row in dtReservations.Rows)
            {
                int userId = Convert.ToInt32(row["userID"]);

                if (!usersDict.ContainsKey(userId))
                {
                    usersDict[userId] = new User
                    {
                        UserID = userId,
                        Username = row["username"].ToString()!,
                        Password = row["password"].ToString()!,
                        CreatedDate = Convert.ToDateTime(row["createdDate"]),
                        UserRole = row["userRole"].ToString()!,
                        FirstName = row["firstName"].ToString()!,
                        LastName = row["lastName"].ToString()!,
                        Email = row["email"].ToString()!,
                        Phone = row["phone"].ToString()!,
                        Reservations = new List<Reservation>(),
                        PaymentInfo = new List<PaymentInfo>()
                    };
                }

                var user = usersDict[userId];

                if (!row.IsNull("reservationID"))
                {
                    var reservation = new Reservation
                    {
                        ReservationID = Convert.ToInt32(row["reservationID"]),
                        UserID = Convert.ToInt32(row["res_userID"]),
                        ReservationType = row["reservationType"].ToString()!,
                        BedroomID = row["bedroomID"] != DBNull.Value ? Convert.ToInt32(row["bedroomID"]) : (int?)null,
                        StartDate = Convert.ToDateTime(row["startDate"]),
                        EndDate = Convert.ToDateTime(row["endDate"]),
                        NumberOfPersons = Convert.ToInt32(row["numberOfPersons"]),
                        ReservationStatus = row["reservationStatus"].ToString()!
                    };

                    user.Reservations!.Add(reservation);
                }
            }

            if (usersDict.Keys.Any())
            {
                var userIds = string.Join(",", usersDict.Keys);
                var paymentSql = $@"
            SELECT * FROM PaymentInfo 
            WHERE UserID IN ({userIds})
            ORDER BY userID, PaymentInfoID";

                var dtPayments = await _db.ExecuteQueryAsync(paymentSql);

                var paymentsByUserId = new Dictionary<int, List<PaymentInfo>>();

                foreach (DataRow row in dtPayments.Rows)
                {
                    var paymentInfo = new PaymentInfo
                    {
                        PaymentInfoID = Convert.ToInt32(row["PaymentInfoID"]),
                        UserID = Convert.ToInt32(row["UserID"]),
                        LastFourDigits = row["LastFourDigits"]?.ToString(),
                        BankHolderName = row["BankHolderName"]?.ToString(),
                        PaymentMethod = row["PaymentMethod"].ToString()!,
                        PaymentToken = row["PaymentToken"]?.ToString()
                    };

                    if (!paymentsByUserId.ContainsKey(paymentInfo.UserID))
                        paymentsByUserId[paymentInfo.UserID] = new List<PaymentInfo>();

                    paymentsByUserId[paymentInfo.UserID].Add(paymentInfo);
                }

                foreach (var userId in usersDict.Keys)
                {
                    if (paymentsByUserId.ContainsKey(userId))
                    {
                        usersDict[userId].PaymentInfo = paymentsByUserId[userId];
                    }
                }
            }

            return Ok(usersDict.Values.ToList());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var sql = @"
                SELECT 
                    u.userID, u.username, u.password, u.createdDate, u.userRole, 
                    u.firstName, u.lastName, u.email, u.phone,
                    r.reservationID, r.userID as res_userID, r.reservationType, 
                    r.bedroomID, r.startDate, r.endDate, r.numberOfPersons, r.reservationStatus
                FROM Users u
                LEFT JOIN Reservation r ON u.userID = r.userID
                WHERE u.userID = @id
                ORDER BY r.startDate";

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@id", id)
            };

            var dt = await _db.ExecuteQueryAsync(sql, parameters);

            if (dt.Rows.Count == 0) return NotFound();

            User? user = null;

            foreach (DataRow row in dt.Rows)
            {
                if (user == null)
                {
                    user = new User
                    {
                        UserID = Convert.ToInt32(row["userID"]),
                        Username = row["username"].ToString()!,
                        Password = row["password"].ToString()!,
                        CreatedDate = Convert.ToDateTime(row["createdDate"]),
                        UserRole = row["userRole"].ToString()!,
                        FirstName = row["firstName"].ToString()!,
                        LastName = row["lastName"].ToString()!,
                        Email = row["email"].ToString()!,
                        Phone = row["phone"].ToString()!,
                        Reservations = new List<Reservation>()
                    };
                }

                if (!row.IsNull("reservationID"))
                {
                    var reservation = new Reservation
                    {
                        ReservationID = Convert.ToInt32(row["reservationID"]),
                        UserID = Convert.ToInt32(row["res_userID"]),
                        ReservationType = row["reservationType"].ToString()!,
                        BedroomID = row["bedroomID"] != DBNull.Value ? Convert.ToInt32(row["bedroomID"]) : (int?)null,
                        StartDate = Convert.ToDateTime(row["startDate"]),
                        EndDate = Convert.ToDateTime(row["endDate"]),
                        NumberOfPersons = Convert.ToInt32(row["numberOfPersons"]),
                        ReservationStatus = row["reservationStatus"].ToString()!
                    };

                    user.Reservations!.Add(reservation);
                }
            }

            return Ok(user);
        }


        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            var sql = @"INSERT INTO Users 
                        (username, password, userRole, firstName, lastName, email, phone) 
                        VALUES 
                        (@username, @password, @userRole, @firstName, @lastName, @email, @phone);
                        SELECT SCOPE_IDENTITY();";

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@username", user.Username),
                new SqlParameter("@password", user.Password),
                new SqlParameter("@userRole", user.UserRole),
                new SqlParameter("@firstName", user.FirstName),
                new SqlParameter("@lastName", user.LastName),
                new SqlParameter("@email", user.Email),
                new SqlParameter("@phone", user.Phone)
            };

            var newId = await _db.ExecuteScalarAsync(sql, parameters);
            user.UserID = newId;

            return CreatedAtAction(nameof(GetUser), new { id = user.UserID }, user);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, User user)
        {
            if (id != user.UserID) return BadRequest();

            var sql = @"UPDATE Users SET
                        username = @username,
                        password = @password,
                        userRole = @userRole,
                        firstName = @firstName,
                        lastName = @lastName,
                        email = @email,
                        phone = @phone
                        WHERE userID = @id";

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@username", user.Username),
                new SqlParameter("@password", user.Password),
                new SqlParameter("@userRole", user.UserRole),
                new SqlParameter("@firstName", user.FirstName),
                new SqlParameter("@lastName", user.LastName),
                new SqlParameter("@email", user.Email),
                new SqlParameter("@phone", user.Phone),
                new SqlParameter("@id", id)
            };

            var affectedRows = await _db.ExecuteNonQueryAsync(sql, parameters);

            if (affectedRows == 0) return NotFound();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var sql = "DELETE FROM Users WHERE userID = @id";
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

// record user , reservationid = 1 , paymentinfoid = 1
// record user , reservationid = 1 , paymentinfoid = 2