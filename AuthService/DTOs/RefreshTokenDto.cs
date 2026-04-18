namespace AuthService.DTOs;

public class RefreshTokenDto
{
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public DateTime AccessTokenExpireAt { get; set; }
    public DateTime RefreshTokenExpireAt { get; set; }
}
