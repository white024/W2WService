using AuthService.DTOs;
using AuthService.Models;
using AuthService.Repositories;
using MongoDB.Driver;
using Shared.Models;
using Shared.Repositories.Base;


namespace AuthService.Services
{
    public class UserService :  IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        public async Task<TokenObjectDto?> LoginAsync(LoginDto dto)
        {
            return new TokenObjectDto();
        }

        public async Task<ReturnObject<UserDto>?> GetByIdAsync(string id)
        {
            return ReturnObject<UserDto>.Success(new UserDto());
        }

        public async Task<ReturnObject<UserDto>?> RegisterAsync(RegisterDto dto)
        {
            return ReturnObject<UserDto>.Success(new UserDto());
        }
        public async Task<ReturnObject<User>?> GetByUserDetails(LoginDto dto)
        {
            return ReturnObject<User>.Success(new User());

        }
    
        private string GenerateToken(User user)
        {
            return "";
        }
    }
}
