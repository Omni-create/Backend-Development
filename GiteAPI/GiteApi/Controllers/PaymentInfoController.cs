using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using GiteApi.Data;
using GiteApi.Models;

namespace GiteApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentInfoController : ControllerBase
    {
        private readonly DatabaseHelper _db;

        public PaymentInfoController(DatabaseHelper db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PaymentInfo>>> GetPaymentInfos()
        {
            var sql = "SELECT * FROM PaymentInfo";
            var dt = await _db.ExecuteQueryAsync(sql);

            var list = new List<PaymentInfo>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new PaymentInfo
                {
                    PaymentInfoID = Convert.ToInt32(row["paymentInfoID"]),
                    UserID = Convert.ToInt32(row["userID"]),
                    LastFourDigits = row["lastFourDigits"]?.ToString(),
                    BankHolderName = row["bankHolderName"]?.ToString(),
                    PaymentMethod = row["paymentMethod"].ToString()!,
                    PaymentToken = row["paymentToken"]?.ToString()
                });
            }

            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentInfo>> GetPaymentInfo(int id)
        {
            var sql = "SELECT * FROM PaymentInfo WHERE paymentInfoID=@id";
            var dt = await _db.ExecuteQueryAsync(sql, new List<SqlParameter> { new SqlParameter("@id", id) });

            if (dt.Rows.Count == 0) return NotFound();

            var row = dt.Rows[0];
            var info = new PaymentInfo
            {
                PaymentInfoID = Convert.ToInt32(row["paymentInfoID"]),
                UserID = Convert.ToInt32(row["userID"]),
                LastFourDigits = row["lastFourDigits"]?.ToString(),
                BankHolderName = row["bankHolderName"]?.ToString(),
                PaymentMethod = row["paymentMethod"].ToString()!,
                PaymentToken = row["paymentToken"]?.ToString()
            };

            return Ok(info);
        }

        [HttpPost]
        public async Task<ActionResult<PaymentInfo>> PostPaymentInfo(PaymentInfo info)
        {
            var sql = @"INSERT INTO PaymentInfo (userID,lastFourDigits,bankHolderName,paymentMethod,paymentToken)
                        VALUES (@userID,@lastFourDigits,@bankHolderName,@paymentMethod,@paymentToken);
                        SELECT SCOPE_IDENTITY();";

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@userID", info.UserID),
                new SqlParameter("@lastFourDigits", (object?)info.LastFourDigits ?? DBNull.Value),
                new SqlParameter("@bankHolderName", (object?)info.BankHolderName ?? DBNull.Value),
                new SqlParameter("@paymentMethod", info.PaymentMethod),
                new SqlParameter("@paymentToken", (object?)info.PaymentToken ?? DBNull.Value)
            };

            info.PaymentInfoID = await _db.ExecuteScalarAsync(sql, parameters);
            return CreatedAtAction(nameof(GetPaymentInfo), new { id = info.PaymentInfoID }, info);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutPaymentInfo(int id, PaymentInfo info)
        {
            if (id != info.PaymentInfoID) return BadRequest();

            var sql = @"UPDATE PaymentInfo SET userID=@userID,lastFourDigits=@lastFourDigits,
                        bankHolderName=@bankHolderName,paymentMethod=@paymentMethod,paymentToken=@paymentToken
                        WHERE paymentInfoID=@id";

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@userID", info.UserID),
                new SqlParameter("@lastFourDigits", (object?)info.LastFourDigits ?? DBNull.Value),
                new SqlParameter("@bankHolderName", (object?)info.BankHolderName ?? DBNull.Value),
                new SqlParameter("@paymentMethod", info.PaymentMethod),
                new SqlParameter("@paymentToken", (object?)info.PaymentToken ?? DBNull.Value),
                new SqlParameter("@id", id)
            };

            await _db.ExecuteNonQueryAsync(sql, parameters);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePaymentInfo(int id)
        {
            var sql = "DELETE FROM PaymentInfo WHERE paymentInfoID=@id";
            await _db.ExecuteNonQueryAsync(sql, new List<SqlParameter> { new SqlParameter("@id", id) });
            return NoContent();
        }
    }
}
