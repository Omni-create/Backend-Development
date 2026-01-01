using Microsoft.AspNetCore.Mvc;
using Backend_Dev.Models;
using Presentation.Transfer;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Presentation.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservedExtraOptionController : ControllerBase
    {
        private readonly IReservedExtraOptionService _reservedExtraOptionService;

        public ReservedExtraOptionController(IReservedExtraOptionService reservedExtraOptionService)
        {
            _reservedExtraOptionService = reservedExtraOptionService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReservedExtraOptionDto>>> GetReservedExtraOptions()
        {
            var reservedExtraOptions = await _reservedExtraOptionService.GetAllReservedExtraOptionsAsync();
            return Ok(reservedExtraOptions);
        }

        [HttpGet("{reservationId}/{extraOptionId}")]
        public async Task<ActionResult<ReservedExtraOptionDto>> GetReservedExtraOption(int reservationId, int extraOptionId)
        {
            var reservedExtraOption = await _reservedExtraOptionService.GetReservedExtraOptionAsync(reservationId, extraOptionId);
            if (reservedExtraOption == null)
                return NotFound();
            return Ok(reservedExtraOption);
        }

        [HttpPost]
        public async Task<ActionResult<ReservedExtraOptionDto>> CreateReservedExtraOption(CreateReservedExtraOptionDto dto)
        {
            var reservedExtraOption = await _reservedExtraOptionService.CreateReservedExtraOptionAsync(dto);
            return CreatedAtAction(nameof(GetReservedExtraOption), new { reservationId = reservedExtraOption.ReservationId, extraOptionId = reservedExtraOption.ExtraOptionId }, reservedExtraOption);
        }

        [HttpDelete("{reservationId}/{extraOptionId}")]
        public async Task<IActionResult> DeleteReservedExtraOption(int reservationId, int extraOptionId)
        {
            await _reservedExtraOptionService.DeleteReservedExtraOptionAsync(reservationId, extraOptionId);
            return NoContent();
        }
    }
}
