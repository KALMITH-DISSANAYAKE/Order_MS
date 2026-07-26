/*using Order_MS.Data;
using Order_MS.DTOs;
using Order_MS.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Order_MS.Services;

public class AuthService : IAuthService
{
    private readonly OrderMSDbContext _context;
    private readonly IConfiguration _config;

    public AuthService(OrderMSDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserName == request.Username);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return null;

        var token = GenerateJwtToken(user);
        var expiresHours = int.Parse(_config["Jwt:ExpireHours"]!);

        return new LoginResponseDto
        {
            Token = token,
            Username = user.UserName,
            FullName = $"{user.FirstName} {user.LastName}",
            Role = user.Role!.RoleName,
            BranchId = user.BranchId,
            ExpiresAt = DateTime.UtcNow.AddHours(expiresHours)
        };
    }

    private string GenerateJwtToken(Models.User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Role, user.Role!.RoleName),
            new Claim("BranchId", user.BranchId?.ToString() ?? "")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresHours = int.Parse(_config["Jwt:ExpireHours"]!);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(expiresHours),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}*/