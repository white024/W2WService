using AuthService.Models;
using AuthService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Attributes;
using Shared.Controllers;
using Shared.Extensions;
using Shared.Models;
using Shared.Services;

namespace AuthService.Controllers;

[ApiController]
[Route("api/[controller]")]
[ApiKeyAuth]
public class UserController : AuthControllerBase
{
    private readonly IUserService _userService;
    private readonly TokenService _tokenService;
    public UserController(IUserService userService, TokenService tokenService)
    {
        _userService = userService;
        _tokenService = tokenService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var requesterId = GetRequesterId(_tokenService);

        try
        {
            dto.IpAddress = IpAddress;
            dto.UserAgent = UserAgent;
            dto.DeviceId  = DeviceId;
            ReturnObject<UserDto>? result = (await _userService.LoginAsync(dto));

            if (result?.Result <= 0)
                return BadRequest($"Hatalı Kullanıcı Bilgisi\n{result}");
            if (result?.Result == 2) return Ok(result);
            if (result?.ResultObject?.RefreshToken == null) return BadRequest("Token Bilgisi Alınamadı");
            result?.ResultObject?.RefreshToken =  Response.AppendRefreshToken(result.ResultObject.RefreshToken);
            if (result?.ResultObject?.Token == null) return BadRequest("Token Bilgisi Alınamadı");
            result?.ResultObject?.Token =  Response.AppendAccessToken(result.ResultObject.Token);
            return Ok(result);

        }
        catch (Exception e)
        {
            await _userService.RevokeAllForUserAsync(requesterId, dto.DeviceId, dto.IpAddress, dto.UserAgent);
            Response.ClearAuthCookies();
            return BadRequest(ReturnObject<UserDto>.Error("Bir Hata Oluştu", e));
        }
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(UserInsertDto dto)
    {
        var requesterId = GetRequesterId(_tokenService);

        try
        {
            dto.IpAddress = IpAddress;
            dto.UserAgent = UserAgent;
            dto.DeviceId  = DeviceId;
            ReturnObject<UserDto>? result = await _userService.RegisterAsync(dto);
            if (result?.Result == 0)
                return BadRequest("Bu email zaten kayıtlı.");
            if (result?.ResultObject?.RefreshToken == null) return BadRequest("Token Bilgisi Alınamadı");
            result?.ResultObject?.RefreshToken =  Response.AppendRefreshToken(result.ResultObject.RefreshToken);
            if (result?.ResultObject?.Token == null) return BadRequest("Token Bilgisi Alınamadı");
            result?.ResultObject?.Token =  Response.AppendAccessToken(result.ResultObject.Token);
            return Ok(result);

        }
        catch (Exception e)
        {
            await _userService.RevokeAllForUserAsync(requesterId, dto.DeviceId, dto.IpAddress, dto.UserAgent);
            Response.ClearAuthCookies();
            return BadRequest(ReturnObject<UserDto>.Error("Bir Hata Oluştu", e));
        }
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        var requesterId = GetRequesterId(_tokenService);

        try
        {
        
            if (string.IsNullOrEmpty(RawRefreshToken))
                return Ok(ReturnObject<UserDto>.Fail("Token Bilgisi Alınamadı, Tekrar Giriş Yapınız"));

            var result = await _userService.RefreshAsync(
                RawRefreshToken,
                DeviceId, IpAddress, UserAgent
                );

            if (result == null)
                return Unauthorized();

            if (result.ResultObject?.RefreshToken == null) return BadRequest("Token Bilgisi Alınamadı");
            result.ResultObject.RefreshToken = Response.AppendRefreshToken(result.ResultObject.RefreshToken);
            if (result?.ResultObject?.Token == null) return BadRequest("Token Bilgisi Alınamadı");
            result.ResultObject.Token =  Response.AppendAccessToken(result.ResultObject.Token);

            return Ok(result);
        }
        catch (Exception e)
        {
            await _userService.RevokeAllForUserAsync(requesterId, DeviceId, IpAddress, UserAgent);
            Response.ClearAuthCookies();
            return BadRequest(ReturnObject<UserDto>.Error("Bir Hata Oluştu, Lütfen Tekrar Giriş Yapınız", e));
        }
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(LogoutDto? dto)
    {
        var requesterId = GetRequesterId(_tokenService);

        try
        {
            if (RawRefreshToken == null) return Unauthorized();
            ReturnObject<bool> result = await _userService.LogoutAsync(RawRefreshToken!, requesterId!, dto);
            Response.ClearAuthCookies();
            return Ok(result);
        }
        catch (Exception e)
        {

            await _userService.RevokeAllForUserAsync(requesterId, DeviceId, IpAddress, UserAgent);
            Response.ClearAuthCookies();
            return BadRequest(ReturnObject<UserDto>.Error("Bir Hata Oluştu", e));
        }

    }

    [HttpGet("profile/{id}")]
    [Authorize]
    public async Task<IActionResult> GetProfile(string id)
    {
        var requesterId = GetRequesterId(_tokenService);
        try
        {

            if (requesterId == id)
            {
                var result = await _userService.GetProfileAsync(id);
                return Ok(result);
            }

            var summary = await _userService.GetUserSummaryAsync(id);
            return Ok(summary);
        }
        catch (Exception e)
        {
            await _userService.RevokeAllForUserAsync(requesterId, DeviceId, IpAddress, UserAgent);
            Response.ClearAuthCookies();
            return BadRequest(ReturnObject<UserDto>.Error("Bir Hata Oluştu", e));

        }

    }

    [HttpPut("profile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UserUpdateDto dto)
    {
        var requesterId = GetRequesterId(_tokenService);

        try
        {
            if (requesterId == null) return Unauthorized();

            var result = await _userService.UpdateProfileAsync(requesterId, dto);
            return Ok(result);
        }
        catch (Exception e)
        {
            await _userService.RevokeAllForUserAsync(requesterId, DeviceId, IpAddress, UserAgent);
            Response.ClearAuthCookies();
            return BadRequest(ReturnObject<UserDto>.Error("Bir Hata Oluştu", e));
        }

    }

    [HttpPut("profile/{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile(string id, [FromBody] UserUpdateDto dto)
    {
        var requesterId =  GetRequesterId(_tokenService);
        try
        {
            if (requesterId == null) return Unauthorized();
            User? checkUser = await _userService.GetUserEntityAsync(requesterId);
            if (checkUser == null) return BadRequest("Kullanıcı Bilgisi Alınamadı");
            if (checkUser.Role != Shared.Enums.UserRole.Admin && checkUser.Role != Shared.Enums.UserRole.Platform) return BadRequest("Yetkisiz işlem");
            var result = await _userService.UpdateProfileAsync(id, dto);
            return Ok(result);
        }
        catch (Exception e)
        {
            await _userService.RevokeAllForUserAsync(requesterId, DeviceId, IpAddress, UserAgent);
            Response.ClearAuthCookies();
            return BadRequest(ReturnObject<UserDto>.Error("Bir Hata Oluştu", e));
        }

    }

    [HttpPut("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] UserChangePasswordDto dto) //TODO: yanlış şifre ile şifre değiştirme işleminde tokeni siliyorum ama sonrasında doğru şifre girerse token almak için ekleme yapılacak
    {
        var requesterId = GetRequesterId(_tokenService);
        try
        {
            if (requesterId == null) return Unauthorized();

            var result = await _userService.ChangePasswordAsync(requesterId, dto);
            await _userService.RevokeAllForUserAsync(requesterId, DeviceId, IpAddress, UserAgent);
            Response.ClearAuthCookies();
            return Ok(result);

        }
        catch (Exception e)
        {
            await _userService.RevokeAllForUserAsync(requesterId, DeviceId, IpAddress, UserAgent);
            Response.ClearAuthCookies();
            return BadRequest(ReturnObject<UserDto>.Error("Bir Hata Oluştu", e));
        }

    }


    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(string id)
    {
        try
        {
            ReturnObject<UserDto>? result = await _userService.GetByIdAsync(id);
            if (result?.Result == 0)
                return BadRequest("Kullanıcı bulunamadı.");
            return Ok(result);

        }
        catch (Exception e)
        {
            return BadRequest(ReturnObject<UserDto>.Error("Bir Hata Oluştu", e));
        }
    }
}
