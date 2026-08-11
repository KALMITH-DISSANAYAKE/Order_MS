using Order_MS.Data;
using Order_MS.DTOs;
using Order_MS.Models;
using Order_MS.Repositories;
using Microsoft.EntityFrameworkCore;
using Order_MS.Exceptions;

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

    public async Task<UserDto> CreateAsync(CreateUserDto dto)
    {

        // Check for duplicate username
        var existing = await _context.Users
       .FirstOrDefaultAsync(u => u.UserName == dto.Username);

        if (existing != null)
            throw new BusinessException($"Username '{dto.Username}' already exists", 409);
        //-----

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

        await _userRepo.AddAsync(user);
        await _userRepo.SaveAsync();  // ← Must save

        // Load navigation properties
        await _context.Entry(user).Reference(u => u.Role).LoadAsync();
        if (user.BranchId.HasValue)
            await _context.Entry(user).Reference(u => u.Branch).LoadAsync();

        return MapToDto(user);
    }

    public async Task<List<UserDto>> GetAllAsync()
    {
        var users = await _context.Users
            .Include(u => u.Role)
            .Include(u => u.Branch)
            .ToListAsync();

        return users.Select(MapToDto).ToList();
    }

    public async Task<UserDto?> GetByIdAsync(int id)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .Include(u => u.Branch)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null) return null;
        return MapToDto(user);
    }

    public async Task<UserDto?> UpdateAsync(int id, UpdateUserDto dto)
    {
        var user = await _userRepo.GetByIdAsync(id);
        if (user == null) 
            throw new BusinessException($"User with ID {id} not found", 404);

        // Check duplicate username (excluding current user)
        var duplicate = await _context.Users
        .FirstOrDefaultAsync(u => u.UserName == dto.Username && u.Id != id);

        if (duplicate != null)
            throw new BusinessException($"Username '{dto.Username}' already taken", 409);

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.UserName = dto.Username;
        user.RoleId = dto.RoleId;
        user.BranchId = dto.BranchId;
        user.ModifiedOn = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        }

        _userRepo.Update(user);        // ← void
        await _userRepo.SaveAsync();   // ← Must save

        await _context.Entry(user).Reference(u => u.Role).LoadAsync();
        if (user.BranchId.HasValue)
            await _context.Entry(user).Reference(u => u.Branch).LoadAsync();

        return MapToDto(user);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var user = await _userRepo.GetByIdAsync(id);
        if (user == null) return false;

        _userRepo.Delete(user);        // ← pass entity, void
        await _userRepo.SaveAsync();   // ← Must save

        return true;
    }

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