using Order_MS.Data;
using Order_MS.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Order_MS.Services;

public class BranchService : IBranchService
{
    private readonly OrderMSDbContext _context;

    public BranchService(OrderMSDbContext context)
    {
        _context = context;
    }

    public async Task<List<BranchDto>> GetAllAsync()
    {
        return await _context.Branches
            .Select(b => new BranchDto
            {
                BranchId = b.BranchId,
                BranchCode = b.BranchCode,
                Location = b.Location
            })
            .ToListAsync();
    }

    public async Task<BranchDto?> GetByIdAsync(int id)
    {
        var branch = await _context.Branches.FindAsync(id);
        if (branch == null) return null;

        return new BranchDto
        {
            BranchId = branch.BranchId,
            BranchCode = branch.BranchCode,
            Location = branch.Location
        };
    }
}