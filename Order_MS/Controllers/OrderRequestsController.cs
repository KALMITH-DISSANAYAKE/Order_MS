using Microsoft.AspNetCore.Mvc;
using Order_MS.DTOs;
using Order_MS.Interfaces;
using Order_MS.Services;

namespace Order_MS.Controllers;


[ApiController]
[Route("api/[controller]")]
public class OrderRequestController : ControllerBase
{

    private readonly IOrderRequestService _orderRequestService;


    public OrderRequestController(IOrderRequestService service)
    {
        _orderRequestService = service;
    }



    [HttpPost]
    public async Task<IActionResult> CreateOrderRequest(CreateOrderRequestDTO dto)
    {

        var result = await _orderRequestService.CreateOrderRequest(dto);


        return Ok(result);

    }

    [HttpGet]
    public async Task<IActionResult> GetAllOrderRequests()
    {
        var result = await _orderRequestService.GetAllOrderRequests();

        return Ok(result);
    }

}