using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Settings;

public class JwtSettings
{
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = "W2W";
    public string Audience { get; set; } = "W2W-Users";
    public int AccessTokenExpirationMinutes { get; set; } = 15;
    public int RefreshTokenExpirationDays { get; set; } = 7;
}