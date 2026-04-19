using AuthService.DTOs;
//using AuthService.Filters;
using AuthService.Models;
using AuthService.Repositories;
using AuthService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Attributes;
using Shared.Extensions;
using Shared.Models;
using System.Security.Claims;
using static Shared.Kafka.Topics.KafkaTopics;

namespace AuthService.Controllers;

[ApiController]
[Route("api/[controller]")]
[ApiKeyAuth]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        try
        {
            dto.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            dto.UserAgent = Request.Headers["User-Agent"].ToString();
            dto.DeviceId  = Request.Headers["X-Device-Id"].ToString();
            ReturnObject<UserDto>? result = (await _userService.LoginAsync(dto));

            if (result?.Result <= 0)
                return BadRequest($"Hatalı Kullanıcı Bilgisi\n{result}");
            if (result?.Result == 2) return Ok(result);
            if (result?.ResultObject?.RefreshToken == null) return BadRequest("Token Bilgisi Alınamadı");
            result?.ResultObject?.RefreshToken =  Response.AppendRefreshToken(result.ResultObject.RefreshToken);
            return Ok(result);

        }
        catch (Exception e)
        {
            return BadRequest(ReturnObject<UserDto>.Error("Bir Hata Oluştu", e));
        }
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        try
        {
            dto.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            dto.UserAgent = Request.Headers["User-Agent"].ToString();
            dto.DeviceId  = Request.Headers["X-Device-Id"].ToString();
            ReturnObject<UserDto>? result = await _userService.RegisterAsync(dto);
            if (result?.Result == 0)
                return BadRequest("Bu email zaten kayıtlı.");
            if (result?.ResultObject?.RefreshToken == null) return BadRequest("Token Bilgisi Alınamadı");

            result?.ResultObject?.RefreshToken =  Response.AppendRefreshToken(result.ResultObject.RefreshToken);
            return Ok(result);

        }
        catch (Exception e)
        {
            return BadRequest(ReturnObject<UserDto>.Error("Bir Hata Oluştu", e));
        }
    }

    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        try
        {
            var rawToken = Request.Cookies["refreshToken"];
            ReturnObject<UserDto>? result = null!;
            if (string.IsNullOrEmpty(rawToken))
                return Ok(ReturnObject<UserDto>.Fail("Taken Bilgisi Alınamadı Tekrar Giriş Yapınız"));

            result =  await _userService.ValidateRefreshTokenAsync(rawToken);

            if (result == null)
            {
                result = await _userService.RefreshAsync(rawToken);
                if (result == null)
                    return Ok(ReturnObject<UserDto>.Fail("Token geçersiz"));

                if (result?.ResultObject?.RefreshToken == null) return BadRequest("Token Bilgisi Alınamadı");

                result?.ResultObject?.RefreshToken =  Response.AppendRefreshToken(result.ResultObject.RefreshToken);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ReturnObject<UserDto>.Error("Bir Hata Oluştu Lütfen Tekrar Giriş Yapınız", ex));
        }


    }
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        var rawToken = Request.Cookies["refreshToken"];

        if (string.IsNullOrEmpty(rawToken))
            return Unauthorized();

        var result = await _userService.RefreshAsync(rawToken);

        if (result == null)
            return Unauthorized();
        if (result?.ResultObject?.RefreshToken == null) return BadRequest("Token Bilgisi Alınamadı");

        result.ResultObject.RefreshToken = Response.AppendRefreshToken(result.ResultObject.RefreshToken);

        return Ok(result);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(LogoutDto? dto)
    {
        var rawToken = Request.Cookies["refreshToken"];
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if(rawToken == null) return Unauthorized();
        ReturnObject<bool> result = await _userService.LogoutAsync(rawToken!, userId!, dto);

        Response.Cookies.Delete("refreshToken");
        return Ok(result);
    }

    //[HttpGet("{id}")]
    //public async Task<IActionResult> GetById(string id)
    //{
    //    try
    //    {
    //        ReturnObject<UserDto>? result = await _userService.GetByIdAsync(id);
    //        if (result?.Result == 0)
    //            return BadRequest("Bu email zaten kayıtlı.");
    //        return Ok(result);

    //    }
    //    catch (Exception e)
    //    {
    //        return BadRequest(ReturnObject<UserDto>.Error("Bir Hata Oluştu", e));
    //    }
    //}
}
