using Microsoft.AspNetCore.Mvc;
using Order_MS.DTOs;
using Order_MS.Services;

namespace Order_MS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransportController : ControllerBase
    {
        private readonly ITransportService _transportService;

        public TransportController(ITransportService transportService)
        {
            _transportService = transportService;
        }

        [HttpGet("order-requests")]
        public async Task<IActionResult> GetApprovedOrderRequests()
        {
            var result = await _transportService.GetApprovedOrderRequestsAsync();
            return Ok(result);
        }

        [HttpGet("available-links")]
        public async Task<IActionResult> GetAvailableDriverVehicleLinks()
        {
            var result = await _transportService.GetAvailableDriverVehicleLinksAsync();
            return Ok(result);
        }

        [HttpGet("assignments")]
        public async Task<IActionResult> GetAllAssignments()
        {
            var result = await _transportService.GetAllAssignmentsAsync();
            return Ok(result);
        }

        [HttpGet("assignments/order-request/{orderReqId:int}")]
        public async Task<IActionResult> GetAssignmentsByOrderRequest(int orderReqId)
        {
            var result = await _transportService.GetAssignmentsByOrderRequestAsync(orderReqId);
            return Ok(result);
        }

  
        [HttpPost("assign")]
        public async Task<IActionResult> AssignTransport([FromBody] AssignTransportDto dto)
        {
            var (success, message, data) = await _transportService.AssignTransportAsync(dto);
            if (!success)
                return BadRequest(new { message });

            return Ok(new { message, assignments = data });
        }

        [HttpPut("assignments/{id:int}/status")]
        public async Task<IActionResult> UpdateAssignmentStatus(
            int id, [FromBody] UpdateAssignmentStatusDto dto)
        {
            var (success, message) = await _transportService.UpdateAssignmentStatusAsync(id, dto);
            if (!success)
                return NotFound(new { message });

            return Ok(new { message });
        }
    }
}