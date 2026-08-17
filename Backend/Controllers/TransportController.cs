using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Order_MS.DTOs;
using Order_MS.Services;
using System.Security.Claims;
using Order_MS.Exceptions;


namespace Order_MS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class TransportController : ControllerBase
    {
        private readonly ITransportService _transportService;

        public TransportController(ITransportService transportService)
        {
            _transportService = transportService;
        }


        [HttpGet("order-requests")]
        //[Authorize(Roles = "TransportDepartment")]
        public async Task<IActionResult> GetApprovedOrderRequests()
        {
            try{
            var result = await _transportService.GetApprovedOrderRequestsAsync();
            return Ok(result);
            }
            catch(Exception ex)
            {
                return StatusCode(500, new {message = "An unexpected error occurred while fetching order requests", details = ex.Message });
            }   
        }

        [HttpGet("available-links")]
        //[Authorize(Roles = "TransportDepartment")]
        public async Task<IActionResult> GetAvailableDriverVehicleLinks()
        {
            try{
            var result = await _transportService.GetAvailableDriverVehicleLinksAsync();
            return Ok(result);
            }
            catch(Exception ex)
            {
                return StatusCode(500, new {message = "An unexpected error occurred while fetching available driver-vehicle links", details = ex.Message });
            }       
        }

        [HttpGet("links")]
        //[Authorize(Roles = "TransportDepartment")]
        public async Task<IActionResult> GetAllDriverVehicleLinks()
        {
            try
            {
                var result = await _transportService.GetAllDriverVehicleLinksAsync();
                return Ok(result);
            }
            catch(Exception ex)
            {
                return StatusCode(500, new {message = "An unexpected error occurred while fetching driver-vehicle links", details = ex.Message });
            }       
        }

        [HttpPost("links")]
        //[Authorize(Roles = "TransportDepartment")]
        public async Task<IActionResult> CreateDriverVehicleLink([FromBody] CreateDriverVehicleLinkDto dto)
        {
            try
            {
                var (success, message, data) = await _transportService.CreateDriverVehicleLinkAsync(dto);
                if (!success)
                    return BadRequest(new { message });

                return Ok(new { message, link = data });
            }
            catch(Exception ex)
            {
                return StatusCode(500, new {message = "An unexpected error occurred while creating driver-vehicle link", details = ex.InnerException?.Message ?? ex.Message });
            }
        }

        [HttpDelete("links/{id:int}")]
        //[Authorize(Roles = "TransportDepartment")]
        public async Task<IActionResult> DeleteDriverVehicleLink(int id)
        {
            try
            {
                var (success, message) = await _transportService.DeleteDriverVehicleLinkAsync(id);
                if (!success)
                {
                    if (message.Contains("assigned to an order"))
                        return BadRequest(new { message });
                    return NotFound(new { message });
                }

                return Ok(new { message });
            }
            catch(Exception ex)
            {
                return StatusCode(500, new {message = "An unexpected error occurred while deleting driver-vehicle link", details = ex.Message });
            }
        }

        [HttpGet("assignments")]
        //[Authorize(Roles = "TransportDepartment")]
        public async Task<IActionResult> GetAllAssignments()
        {
            try
            {
            var result = await _transportService.GetAllAssignmentsAsync();
            return Ok(result);
            }
            catch(Exception ex)
            {
                return StatusCode(500, new {message = "An unexpected error occurred while fetching assignments", details = ex.Message });
            }   
        }

        [HttpGet("assignments/order-request/{orderReqId:int}")]
        //[Authorize(Roles = "TransportDepartment")]
        public async Task<IActionResult> GetAssignmentsByOrderRequest(int orderReqId)
        {
            try{
            var result = await _transportService.GetAssignmentsByOrderRequestAsync(orderReqId);
            return Ok(result);
            }
            catch(Exception ex)
            {
                return StatusCode(500, new {message = "An unexpected error occurred while fetching assignments for order request", details = ex.Message });
            }   
        }
  
        [HttpPost("assign")]
        //[Authorize(Roles = "TransportDepartment")]
        public async Task<IActionResult> AssignTransport([FromBody] AssignTransportDto dto)
        {
            try
            {
                var (success, message, data) = await _transportService.AssignTransportAsync(dto);
                if (!success)
                    return BadRequest(new { message });

                return Ok(new { message, assignments = data });
            }
            catch(BusinessException ex)
            {
                return StatusCode(400, new {message = ex.Message});
            }
            catch(Exception ex)
            {
                return StatusCode(500, new {message = "An unexpected error occurred while assigning transport", details = ex.Message });
            }
        }

        [HttpPut("assignments/{id:int}/status")]
        //[Authorize(Roles = "TransportDepartment")]
        public async Task<IActionResult> UpdateAssignmentStatus(
            int id, [FromBody] UpdateAssignmentStatusDto dto)
        {
            try{
            var (success, message) = await _transportService.UpdateAssignmentStatusAsync(id, dto);
            if (!success)
                return NotFound(new { message });

            return Ok(new { message });
            }
            catch(BusinessException ex)
            {
                return StatusCode(400, new {message = ex.Message});
            }
            catch(Exception ex)
            {
                return StatusCode(500, new {message = "An unexpected error occurred while updating assignment status    ", details = ex.Message });
            }   
        }

        // --- Vehicles ---

        [HttpGet("vehicles")]
        //[Authorize(Roles = "TransportDepartment")]
        public async Task<IActionResult> GetAllVehicles()
        {
            try
            {
                var result = await _transportService.GetAllVehiclesAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching vehicles", details = ex.Message });
            }
        }

        [HttpPost("vehicles")]
        //[Authorize(Roles = "TransportDepartment")]
        public async Task<IActionResult> CreateVehicle([FromBody] CreateVehicleDto dto)
        {
            try
            {
                var (success, message, data) = await _transportService.CreateVehicleAsync(dto);
                if (!success)
                    return BadRequest(new { message });

                return Ok(new { message, vehicle = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating vehicle", details = ex.InnerException?.Message ?? ex.Message });
            }
        }

        [HttpPut("vehicles/{id:int}")]
        //[Authorize(Roles = "TransportDepartment")]
        public async Task<IActionResult> UpdateVehicle(int id, [FromBody] UpdateVehicleDto dto)
        {
            try
            {
                var (success, message, data) = await _transportService.UpdateVehicleAsync(id, dto);
                if (!success)
                    return NotFound(new { message });

                return Ok(new { message, vehicle = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating vehicle", details = ex.InnerException?.Message ?? ex.Message });
            }
        }

        // --- Drivers ---

        [HttpGet("drivers")]
        //[Authorize(Roles = "TransportDepartment")]
        public async Task<IActionResult> GetAllDrivers()
        {
            try
            {
                var result = await _transportService.GetAllDriversAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching drivers", details = ex.Message });
            }
        }

        [HttpPost("drivers")]
        //[Authorize(Roles = "TransportDepartment")]
        public async Task<IActionResult> CreateDriver([FromBody] CreateDriverDto dto)
        {
            try
            {
                var (success, message, data) = await _transportService.CreateDriverAsync(dto);
                if (!success)
                    return BadRequest(new { message });

                return Ok(new { message, driver = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating driver", details = ex.InnerException?.Message ?? ex.Message });
            }
        }
        [HttpPut("drivers/{id:int}")]
        //[Authorize(Roles = "TransportDepartment")]
        public async Task<IActionResult> UpdateDriver(int id, [FromBody] UpdateDriverDto dto)
        {
            try
            {
                var (success, message, data) = await _transportService.UpdateDriverAsync(id, dto);
                if (!success)
                    return NotFound(new { message });

                return Ok(new { message, driver = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating driver", details = ex.InnerException?.Message ?? ex.Message });
            }
        }
    }
}
