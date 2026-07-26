using Microsoft.AspNetCore.Mvc;
using Order_MS.DTOs;
using Order_MS.Services;

namespace Order_MS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeliveryController : ControllerBase
    {
        private readonly IDeliveryService _deliveryService;

        public DeliveryController(IDeliveryService deliveryService)
        {
            _deliveryService = deliveryService;
        }

        [HttpPost("verify")]
        public async Task<IActionResult> VerifyDelivery([FromBody] VerifyDeliveryDto dto)
        {
            try
            {
                var result = await _deliveryService.VerifyDeliveryAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("status")]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateDeliveryStatusDto dto)
        {
            try
            {
                var result = await _deliveryService.UpdateDeliveryStatusAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory()
        {
            var history = await _deliveryService.GetDeliveryHistoryAsync();
            return Ok(history);
        }

        [HttpPost("confirm/{orderId}")]
        public async Task<IActionResult> ConfirmDelivery(int orderId)
        {
            try
            {
                var result = await _deliveryService.ConfirmDeliveryAsync(orderId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}