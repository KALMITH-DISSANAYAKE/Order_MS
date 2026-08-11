using Order_MS.DTOs;

namespace Order_MS.Services;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto request);
}