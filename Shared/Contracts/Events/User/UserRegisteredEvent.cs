using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Contracts.Events.User;


public class UserRegisteredEvent
{
    public string UserId { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Name { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Surname { get; set; }
    public string? InviteCode { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}