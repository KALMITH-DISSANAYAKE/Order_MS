using Order_MS.Data;
using Order_MS.DTOs;
using Order_MS.Models;
using Order_MS.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Order_MS.Services;

public class BranchService : IBranchService
{
    private readonly OrderMSDbContext _context;
    private readonly IGenericRepository<Branch> _branchRepo;

    public BranchService(OrderMSDbContext context, IGenericRepository<Branch> branchRepo)
    {
        _context = context;
        _branchRepo = branchRepo;
    }

    public async Task<List<BranchDto>> GetAllAsync()
    {
        var branches = await _branchRepo.GetAllAsync();
        return branches.Select(b => new BranchDto
        {
            BranchId = b.BranchId,
            BranchCode = b.BranchCode,
            Location = b.Location
        }).ToList();
    }

    public async Task<BranchDto?> GetByIdAsync(int id)
    {
        var branch = await _branchRepo.GetByIdAsync(id);
        if (branch == null) return null;

        return new BranchDto
        {
            BranchId = branch.BranchId,
            BranchCode = branch.BranchCode,
            Location = branch.Location
        };
    }

    public async Task<BranchDto> CreateAsync(CreateBranchDto dto)
    {
        var branch = new Branch
        {
            BranchCode = dto.BranchCode,
            Location = dto.Location,
            CreatedOn = DateTime.UtcNow
        };

        await _branchRepo.AddAsync(branch);
        await _branchRepo.SaveAsync();  // ← NEW: Must call SaveAsync!

        return new BranchDto
        {
            BranchId = branch.BranchId,  // EF populates ID after AddAsync
            BranchCode = branch.BranchCode,
            Location = branch.Location
        };
    }

    public async Task<BranchDto?> UpdateAsync(int id, UpdateBranchDto dto)
    {
        var branch = await _branchRepo.GetByIdAsync(id);
        if (branch == null) return null;

        branch.BranchCode = dto.BranchCode;
        branch.Location = dto.Location;
        branch.ModifiedOn = DateTime.UtcNow;

        _branchRepo.Update(branch);      // ← void, just marks as modified
        await _branchRepo.SaveAsync();     // ← NEW: Must call SaveAsync!

        return new BranchDto
        {
            BranchId = branch.BranchId,
            BranchCode = branch.BranchCode,
            Location = branch.Location
        };
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var branch = await _branchRepo.GetByIdAsync(id);
        if (branch == null) return false;

        _branchRepo.Delete(branch);        // ← void, pass entity not int
        await _branchRepo.SaveAsync();     // ← NEW: Must call SaveAsync!

        return true;
    }
}