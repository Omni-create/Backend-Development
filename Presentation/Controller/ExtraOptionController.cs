using Microsoft.AspNetCore.Mvc;
using Backend_Dev.Models;
using Presentation.Transfer;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Presentation.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExtraOptionController : ControllerBase
    {
        private readonly IExtraOptionService _extraOptionService;

        public ExtraOptionController(IExtraOptionService extraOptionService)
        {
            _extraOptionService = extraOptionService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ExtraOptionDto>>> GetExtraOptions()
        {
            var extraOptions = await _extraOptionService.GetAllExtraOptionsAsync();
            return Ok(extraOptions);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ExtraOptionDto>> GetExtraOption(int id)
        {
            var extraOption = await _extraOptionService.GetExtraOptionByIdAsync(id);
            if (extraOption == null)
                return NotFound();
            return Ok(extraOption);
        }

        [HttpPost]
        public async Task<ActionResult<ExtraOptionDto>> CreateExtraOption(CreateExtraOptionDto dto)
        {
            var extraOption = await _extraOptionService.CreateExtraOptionAsync(dto);
            return CreatedAtAction(nameof(GetExtraOption), new { id = extraOption.ExtraOptionId }, extraOption);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateExtraOption(int id, CreateExtraOptionDto dto)
        {
            await _extraOptionService.UpdateExtraOptionAsync(id, dto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExtraOption(int id)
        {
            await _extraOptionService.DeleteExtraOptionAsync(id);
            return NoContent();
        }
    }
}
