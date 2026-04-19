namespace AuthService.DTOs;

public class InviteDto
{
    public string CompanyId { get; set; } = null!;
    public string? CompanyName { get; set; }
    public string? UserId { get; set; } = null!;
    public DateTime? ExpireAt { get; set; } = DateTime.UtcNow.AddDays(30);
    public bool? IsUsed { get; set; } = false;
}
