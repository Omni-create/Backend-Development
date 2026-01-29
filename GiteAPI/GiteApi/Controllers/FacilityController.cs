using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using GiteApi.Data;
using GiteApi.Models;

namespace GiteApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FacilitiesController : ControllerBase
    {
        private readonly DatabaseHelper _db;

        public FacilitiesController(DatabaseHelper db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Facility>>> GetFacilities()
        {
            var sql = "SELECT * FROM Facilities";
            var dt = await _db.ExecuteQueryAsync(sql);

            var facilities = new List<Facility>();
            foreach (DataRow row in dt.Rows)
            {
                facilities.Add(new Facility
                {
                    FacilityID = Convert.ToInt32(row["facilityID"]),
                    FacilityName = row["facilityName"].ToString()!,
                    Price = Convert.ToDecimal(row["price"])
                });
            }

            return Ok(facilities);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Facility>> GetFacility(int id)
        {
            var sql = "SELECT * FROM Facilities WHERE facilityID=@id";
            var dt = await _db.ExecuteQueryAsync(sql, new List<SqlParameter> { new SqlParameter("@id", id) });

            if (dt.Rows.Count == 0) return NotFound();

            var row = dt.Rows[0];
            var facility = new Facility
            {
                FacilityID = Convert.ToInt32(row["facilityID"]),
                FacilityName = row["facilityName"].ToString()!,
                Price = Convert.ToDecimal(row["price"])
            };

            return Ok(facility);
        }

        [HttpPost]
        public async Task<ActionResult<Facility>> PostFacility(Facility facility)
        {
            var sql = @"INSERT INTO Facilities (facilityName, price)
                        VALUES (@facilityName, @price);
                        SELECT SCOPE_IDENTITY();";

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@facilityName", facility.FacilityName),
                new SqlParameter("@price", facility.Price)
            };

            facility.FacilityID = await _db.ExecuteScalarAsync(sql, parameters);
            return CreatedAtAction(nameof(GetFacility), new { id = facility.FacilityID }, facility);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutFacility(int id, Facility facility)
        {
            if (id != facility.FacilityID) return BadRequest();

            var sql = @"UPDATE Facilities SET facilityName=@facilityName, price=@price
                        WHERE facilityID=@id";

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@facilityName", facility.FacilityName),
                new SqlParameter("@price", facility.Price),
                new SqlParameter("@id", id)
            };

            await _db.ExecuteNonQueryAsync(sql, parameters);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFacility(int id)
        {
            var sql = "DELETE FROM Facilities WHERE facilityID=@id";
            await _db.ExecuteNonQueryAsync(sql, new List<SqlParameter> { new SqlParameter("@id", id) });
            return NoContent();
        }
    }
}
