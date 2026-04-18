using AuthService.DTOs;
using AuthService.Models;
using AuthService.Repositories;
using AuthService.Services;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;
using static Shared.Kafka.Topics.KafkaTopics;

namespace AuthService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly TokenService _tokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    public UserController(IUserService userService, TokenService tokenService, IRefreshTokenRepository refreshTokenRepository)
    {
        _userService = userService;
        _tokenService=tokenService;
        _refreshTokenRepository = refreshTokenRepository;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        try
        {
            TokenObjectDto? result = (await _userService.LoginAsync(dto));/////////hatta Token object gg silecem ne gerek var amk

            ReturnObject<UserDto>? UserObject = result?.UserDtoObject;
            if (UserObject?.Result == 0)
                return BadRequest("Hatalı Kullanıcı Bilgisi");
            ////////////7loginasyncye taşınacak//////////////
            if (result == null || result?.UserId == null || result?.CompanyId == null) return BadRequest("Kullanıcı Bilgilerine Erişilemedi"); //return x veya throw
            var token = _tokenService.CreateAccessToken(result?.UserId!, result?.CompanyId!);
            UserObject?.ResultObject?.Token = token;
            var refreshToken = await _tokenService.CreateRefreshToken(result?.UserId!, result?.CompanyId!);
            UserObject?.ResultObject?.RefreshToken = refreshToken.rawToken;
            await _refreshTokenRepository.SaveAsync(refreshToken.entity);
            ////////////7loginasyncye taşınacak//////////////

            return Ok(UserObject);

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
            ReturnObject<UserDto>? result = await _userService.RegisterAsync(dto);
            if (result?.Result == 0)
                return BadRequest("Bu email zaten kayıtlı.");
            return Ok(result);

        }
        catch (Exception e)
        {
            return BadRequest(ReturnObject<UserDto>.Error("Bir Hata Oluştu", e));
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        try
        {
            ReturnObject<UserDto>? result = await _userService.GetByIdAsync(id);
            if (result?.Result == 0)
                return BadRequest("Bu email zaten kayıtlı.");
            return Ok(result);

        }
        catch (Exception e)
        {
            return BadRequest(ReturnObject<UserDto>.Error("Bir Hata Oluştu", e));
        }
    }
}
