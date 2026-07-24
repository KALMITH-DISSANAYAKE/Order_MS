using Order_MS.DTOs;
using Order_MS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Order_MS.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Must be logged in to access any branch endpoint
public class BranchesController : ControllerBase
{
    private readonly IBranchService _branchService;

    public BranchesController(IBranchService branchService)
    {
        _branchService = branchService;
    }

    // GET /api/branches
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var branches = await _branchService.GetAllAsync();
        return Ok(branches);
    }

    // GET /api/branches/1
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var branch = await _branchService.GetByIdAsync(id);
        if (branch == null)
            return NotFound(new { message = "Branch not found" });

        return Ok(branch);
    }
}