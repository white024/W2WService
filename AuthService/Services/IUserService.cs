using AuthService.DTOs;
using AuthService.Models;
using Shared.Models;

namespace AuthService.Services;

public interface IUserService
{
    Task<ReturnObject<UserDto>?> LoginAsync(LoginDto dto);
    Task<ReturnObject<UserDto>?> RegisterAsync(UserInsertDto dto);
    Task<ReturnObject<UserDto>?> RefreshAsync(string? refreshToken, string? deviceId, string? ipAddress, string? userAgent);
    Task<ReturnObject<bool>> LogoutAsync(string rawToken, string userId, LogoutDto? dto);
    Task<ReturnObject<UserDto>> GetProfileAsync(string userId);
    Task<ReturnObject<UserSummaryDto>> GetUserSummaryAsync(string userId);
    Task<ReturnObject<UserDto>> UpdateProfileAsync(string userId, UserUpdateDto dto);
    Task<ReturnObject<bool>> ChangePasswordAsync(string userId, UserChangePasswordDto dto);
    Task<User?> GetUserEntityAsync(string id);
    Task<ReturnObject<UserDto>?> GetByIdAsync(string id);
    Task RevokeAllForUserAsync(string? userId, string? deviceId, string? ipAdress, string? userAgent);
}
