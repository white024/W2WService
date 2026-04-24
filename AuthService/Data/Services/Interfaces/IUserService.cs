using AuthService.Models.DTOs;
using AuthService.Models.Entities;
using Shared.Models;

namespace AuthService.Data.Services.Interfaces
{
    public interface IUserService
    {
        Task<ReturnObject<UserDto>> LoginAsync(UserLoginDto dto, CancellationToken ct = default);
        Task<ReturnObject<UserDto>> RegisterAsync(UserCreateDto dto, CancellationToken ct = default);
        Task<ReturnObject<UserDto>?> RefreshAsync(string refreshToken, string? deviceId, string? ipAddress, string? userAgent, CancellationToken ct = default);
        Task<ReturnObject<bool>> LogoutAsync(string rawToken, string userId, LogoutDto? dto = null, CancellationToken ct = default);
        Task RevokeAllAsync(string? userId, string? ipAddress, string? deviceId, string? userAgent, CancellationToken ct = default);
        Task<ReturnObject<UserDto>> GetProfileAsync(string userId, CancellationToken ct = default);
        Task<ReturnObject<UserDto>> UpdateProfileAsync(string userId, UserUpdateDto dto, CancellationToken ct = default);
        Task<ReturnObject<bool>> ChangePasswordAsync(string userId, UserChangePasswordDto dto, CancellationToken ct = default);
        Task<ReturnObject<UserDto>?> GetByIdAsync(string id, CancellationToken ct = default);

        Task<User?> GetUserEntityAsync(string id, CancellationToken ct = default);

    }
}
