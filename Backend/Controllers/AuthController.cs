using Order_MS.DTOs;
using Order_MS.Services;
using Microsoft.AspNetCore.Mvc;

namespace Order_MS.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var result = await _authService.LoginAsync(request);
        /*if (result == null)
            return Unauthorized(new { message = "Invalid username or password" });
        */
        return Ok(result);
    }
}