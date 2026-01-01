using Microsoft.AspNetCore.Mvc;
using Backend_Dev.Models;
using Presentation.Transfer;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Presentation.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentInfoController : ControllerBase
    {
        private readonly IPaymentInfoService _paymentInfoService;

        public PaymentInfoController(IPaymentInfoService paymentInfoService)
        {
            _paymentInfoService = paymentInfoService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PaymentInfoDto>>> GetPaymentInfos()
        {
            var paymentInfos = await _paymentInfoService.GetAllPaymentInfosAsync();
            return Ok(paymentInfos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentInfoDto>> GetPaymentInfo(int id)
        {
            var paymentInfo = await _paymentInfoService.GetPaymentInfoByIdAsync(id);
            if (paymentInfo == null)
                return NotFound();
            return Ok(paymentInfo);
        }

        [HttpPost]
        public async Task<ActionResult<PaymentInfoDto>> CreatePaymentInfo(CreatePaymentInfoDto dto)
        {
            var paymentInfo = await _paymentInfoService.CreatePaymentInfoAsync(dto);
            return CreatedAtAction(nameof(GetPaymentInfo), new { id = paymentInfo.PaymentInfoId }, paymentInfo);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePaymentInfo(int id, CreatePaymentInfoDto dto)
        {
            await _paymentInfoService.UpdatePaymentInfoAsync(id, dto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePaymentInfo(int id)
        {
            await _paymentInfoService.DeletePaymentInfoAsync(id);
            return NoContent();
        }
    }
}
