using AuthService.DTOs;
using AuthService.Models;
using Shared.Models;

namespace AuthService.Services
{
    public interface IUserService
    {
        Task<TokenObjectDto?> LoginAsync(LoginDto dto);
        Task<ReturnObject<UserDto>?> RegisterAsync(RegisterDto dto);
        Task<ReturnObject<UserDto>?> GetByIdAsync(string id);
        Task<ReturnObject<User>?> GetByUserDetails(LoginDto dto);
    }
}
