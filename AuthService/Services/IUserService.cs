using AuthService.DTOs;
using AuthService.Models;
using Shared.Models;

namespace AuthService.Services;

public interface IUserService
{
    Task<ReturnObject<UserDto>?> LoginAsync(LoginDto dto);
    Task<ReturnObject<UserDto>?> RegisterAsync(RegisterDto dto);
    Task<ReturnObject<UserDto>?> RefreshAsync(string? refreshToken);
    Task<ReturnObject<UserDto>?> ValidateRefreshTokenAsync(string rawToken);
    Task<ReturnObject<bool>> LogoutAsync(string rawToken, string userId, LogoutDto? dto);
    //Task<ReturnObject<UserDto>?> GetByIdAsync(string id);
}
