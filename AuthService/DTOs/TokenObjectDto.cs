using Shared.Models;

namespace AuthService.DTOs
{
    public class TokenObjectDto
    {
        public string UserId { get; set; } = null!;
        public string CompanyId { get; set; } = null!;
        public ReturnObject<UserDto>? UserDtoObject { get; set; }
    }
}
