using Order_MS.DTOs;

namespace Order_MS.Services;

public interface IUserService
{
    Task<UserDto> CreateAsync(CreateUserDto dto);
    Task<List<UserDto>> GetAllAsync();
    Task<UserDto?> GetByIdAsync(int id);
    Task<UserDto?> UpdateAsync(int id, UpdateUserDto dto);
    Task<bool> DeleteAsync(int id);
}