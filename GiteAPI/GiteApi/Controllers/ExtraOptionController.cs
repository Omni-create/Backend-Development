using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using GiteApi.Data;
using GiteApi.Models;

namespace GiteApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExtraOptionController : ControllerBase
    {
        private readonly DatabaseHelper _db;

        public ExtraOptionController(DatabaseHelper db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ExtraOption>>> GetExtraOptions()
        {
            var sql = "SELECT * FROM ExtraOptions ORDER BY ExtraOptionId";
            var dt = await _db.ExecuteQueryAsync(sql);

            var extraOptions = new List<ExtraOption>();
            foreach (DataRow row in dt.Rows)
            {
                extraOptions.Add(new ExtraOption
                {
                    ExtraOptionId = Convert.ToInt32(row["ExtraOptionId"]),
                    OptionName = row["OptionName"].ToString()!,  // Changed from "Name" to "OptionName"
                    Price = Convert.ToDecimal(row["Price"])
                    // Removed Description and Category since they don't exist in the model
                });
            }

            return Ok(extraOptions);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ExtraOption>> GetExtraOption(int id)
        {
            var sql = "SELECT * FROM ExtraOptions WHERE ExtraOptionId = @id";
            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@id", id)
            };

            var dt = await _db.ExecuteQueryAsync(sql, parameters);

            if (dt.Rows.Count == 0) return NotFound();

            var row = dt.Rows[0];
            var extraOption = new ExtraOption
            {
                ExtraOptionId = Convert.ToInt32(row["ExtraOptionId"]),
                OptionName = row["OptionName"].ToString()!,  // Changed from "Name" to "OptionName"
                Price = Convert.ToDecimal(row["Price"])
                // Removed Description and Category since they don't exist in the model
            };

            return Ok(extraOption);
        }

        [HttpPost]
        public async Task<ActionResult<ExtraOption>> PostExtraOption(ExtraOption extraOption)
        {
            var sql = @"INSERT INTO ExtraOptions (OptionName, Price) 
                        VALUES (@OptionName, @Price);
                        SELECT SCOPE_IDENTITY();";

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@OptionName", extraOption.OptionName),  // Changed from "@Name" to "@OptionName"
                new SqlParameter("@Price", extraOption.Price)
                // Removed Description and Category parameters
            };

            var newId = await _db.ExecuteScalarAsync(sql, parameters);
            extraOption.ExtraOptionId = Convert.ToInt32(newId);

            return CreatedAtAction(nameof(GetExtraOption), new { id = extraOption.ExtraOptionId }, extraOption);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutExtraOption(int id, ExtraOption extraOption)
        {
            if (id != extraOption.ExtraOptionId) return BadRequest();

            var sql = @"UPDATE ExtraOptions SET 
                        OptionName = @OptionName,
                        Price = @Price
                        WHERE ExtraOptionId = @id";

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@OptionName", extraOption.OptionName),  // Changed from "@Name" to "@OptionName"
                new SqlParameter("@Price", extraOption.Price),
                new SqlParameter("@id", id)
                // Removed Description and Category parameters
            };

            var affectedRows = await _db.ExecuteNonQueryAsync(sql, parameters);

            if (affectedRows == 0) return NotFound();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExtraOption(int id)
        {
            var sql = "DELETE FROM ExtraOptions WHERE ExtraOptionId = @id";
            var parameters = new List<SqlParameter> { new SqlParameter("@id", id) };

            var affectedRows = await _db.ExecuteNonQueryAsync(sql, parameters);

            if (affectedRows == 0) return NotFound();

            return NoContent();
        }
    }
}