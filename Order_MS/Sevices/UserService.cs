using Order_MS.Data;
using Order_MS.DTOs;
using Order_MS.Models;
using Order_MS.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Order_MS.Services;

public class UserService : IUserService
{
    private readonly OrderMSDbContext _context;
    private readonly IGenericRepository<User> _userRepo;

    public UserService(OrderMSDbContext context, IGenericRepository<User> userRepo)
    {
        _context = context;
        _userRepo = userRepo;
    }

    // ========== CREATE ==========
    public async Task<UserDto> CreateAsync(CreateUserDto dto)
    {
        // Hash password with BCrypt (automatically handles salt)
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        var user = new User
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            UserName = dto.Username,
            PasswordHash = passwordHash,
            RoleId = dto.RoleId,
            BranchId = dto.BranchId,
            CreatedOn = DateTime.UtcNow
        };

        var created = await _userRepo.AddAsync(user);

        // Load navigation properties for response
        await _context.Entry(created).Reference(u => u.Role).LoadAsync();
        if (created.BranchId.HasValue)
            await _context.Entry(created).Reference(u => u.Branch).LoadAsync();

        return MapToDto(created);
    }

    // ========== READ ALL ==========
    public async Task<List<UserDto>> GetAllAsync()
    {
        var users = await _context.Users
            .Include(u => u.Role)
            .Include(u => u.Branch)
            .ToListAsync();

        return users.Select(MapToDto).ToList();
    }

    // ========== READ ONE ==========
    public async Task<UserDto?> GetByIdAsync(int id)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .Include(u => u.Branch)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null) return null;

        return MapToDto(user);
    }

    // ========== UPDATE ==========
    public async Task<UserDto?> UpdateAsync(int id, UpdateUserDto dto)
    {
        var user = await _userRepo.GetByIdAsync(id);
        if (user == null) return null;

        // Update basic info
        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.UserName = dto.Username;
        user.RoleId = dto.RoleId;
        user.BranchId = dto.BranchId;
        user.ModifiedOn = DateTime.UtcNow;

        // Only hash new password if provided
        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        }

        await _userRepo.UpdateAsync(user);

        // Reload navigation properties
        await _context.Entry(user).Reference(u => u.Role).LoadAsync();
        if (user.BranchId.HasValue)
            await _context.Entry(user).Reference(u => u.Branch).LoadAsync();

        return MapToDto(user);
    }

    // ========== DELETE ==========
    public async Task<bool> DeleteAsync(int id)
    {
        if (!await _userRepo.ExistsAsync(id))
            return false;

        await _userRepo.DeleteAsync(id);
        return true;
    }

    // ========== PRIVATE HELPER: Map Model → DTO ==========
    private static UserDto MapToDto(User user)
    {
        return new UserDto
        {
            UserId = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Username = user.UserName,
            RoleId = user.RoleId,
            RoleName = user.Role?.RoleName ?? "Unknown",
            BranchId = user.BranchId,
            BranchName = user.Branch?.Location,
            CreatedOn = user.CreatedOn
        };
    }
}