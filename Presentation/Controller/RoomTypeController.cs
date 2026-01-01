using Microsoft.AspNetCore.Mvc;
using Backend_Dev.Models;
using Presentation.Transfer;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Presentation.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoomTypeController : ControllerBase
    {
        private readonly IRoomTypeService _roomTypeService;

        public RoomTypeController(IRoomTypeService roomTypeService)
        {
            _roomTypeService = roomTypeService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoomTypeDto>>> GetRoomTypes()
        {
            var roomTypes = await _roomTypeService.GetAllRoomTypesAsync();
            return Ok(roomTypes);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RoomTypeDto>> GetRoomType(int id)
        {
            var roomType = await _roomTypeService.GetRoomTypeByIdAsync(id);
            if (roomType == null)
                return NotFound();
            return Ok(roomType);
        }

        [HttpPost]
        public async Task<ActionResult<RoomTypeDto>> CreateRoomType(CreateRoomTypeDto dto)
        {
            var roomType = await _roomTypeService.CreateRoomTypeAsync(dto);
            return CreatedAtAction(nameof(GetRoomType), new { id = roomType.RoomTypeId }, roomType);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRoomType(int id, CreateRoomTypeDto dto)
        {
            await _roomTypeService.UpdateRoomTypeAsync(id, dto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoomType(int id)
        {
            await _roomTypeService.DeleteRoomTypeAsync(id);
            return NoContent();
        }
    }
}
