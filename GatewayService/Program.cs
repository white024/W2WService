using GatewayService.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Shared.Extensions;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Yarp.ReverseProxy.Configuration;

var builder = WebApplication.CreateBuilder(args);

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();


builder.Services.AddSharedTokenService();
builder.Services.AddSharedJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorization();
builder.Services.AddHttpClient();
builder.Services.AddServiceDiscovery();
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddServiceDiscoveryDestinationResolver();

var app = builder.Build();

app.UseMiddleware<RefreshTokenMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapReverseProxy();

await app.RunAsync();