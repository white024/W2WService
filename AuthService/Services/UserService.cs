using AuthService.DTOs;
using AuthService.Models;
using AuthService.Repositories;
using AutoMapper;
using MongoDB.Bson;
using MongoDB.Driver;
using Shared.Enums;
using Shared.Models;

namespace AuthService.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IInviteRepository _inviteRepository;
    private readonly ICompanyService _companyService;
    private readonly TokenService _tokenService;
    private readonly IMapper _mapper;

    public UserService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IInviteRepository inviteRepository,
        ICompanyService companyService,
        TokenService tokenService,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _inviteRepository = inviteRepository;
        _companyService = companyService;
        _tokenService = tokenService;
        _mapper = mapper;
    }

    public async Task<ReturnObject<UserDto>?> LoginAsync(LoginDto dto)
    {
        var user = await ResolveUserAsync(dto.UserName, dto.Password);

        var userCompany = ResolveUserCompany(user, dto.CompanyId);
        if (userCompany == null)
        {
            return ReturnObject<UserDto>.SendResponse(
                "Company selection required",
                user?.Companies!
            );
        }

        await SetActiveCompanyAsync(user, userCompany);

        return ReturnObject<UserDto>.Success(await BuildUserResponseAsync(user, userCompany, dto.DeviceId, dto.IpAddress, dto.UserAgent));
    }

    public async Task<ReturnObject<UserDto>?> RegisterAsync(RegisterDto dto)
    {
        await CheckUserExistsAsync(dto);

        var invite = await ResolveInviteAsync(dto.InviteToken);
        if (!string.IsNullOrEmpty(dto.InviteToken) && invite == null) throw new Exception("Invalid Invite Token");
        var user = _mapper.Map<User>(dto);
        if (user == null) throw new Exception("Mapping failed");
        user.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        var company = await _companyService.CreateIfNotFoundCompanyAsync(
            invite?.CompanyId,
            invite?.CompanyName
        );

        user?.Companies?.Add(new UserCompany
        {
            Id = company.Id,
            CompanyName = company.CompanyName,
            Role = CompanyRole.Viewer,
            IsActive = true,
            JoinedAt = DateTime.UtcNow
        });

        var result = await _userRepository.AddAsync(user!);
        if (result == null) throw new Exception("User could not be saved");

        if (invite != null)
        {
            invite.IsUsed = true;
            await _inviteRepository.UpdateAsync(invite);
        }

        var userCompany = result?.Companies?.FirstOrDefault(x => x.IsActive);
        if (userCompany == null) throw new Exception("Active company could not be assigned");
        var userDto = await BuildUserResponseAsync(result!, userCompany);
        return ReturnObject<UserDto>.Success(userDto);
    }


    private async Task<User> ResolveUserAsync(string userName, string password)
    {
        var users = (await _userRepository.FindAsync(
            x => x.UserName == userName ||
                 x.Email == userName ||
                 x.PhoneNumber == userName
        )).ToList();

        if (!users.Any())
            throw new Exception("Invalid credentials");

        var user = users.FirstOrDefault(u => BCrypt.Net.BCrypt.Verify(password, u.Password));
        if (user == null)
            throw new Exception("Invalid credentials");

        return user;
    }

    private UserCompany? ResolveUserCompany(User user, string? companyId)
    {
        if (user?.Companies == null || !user.Companies.Any())
            throw new Exception("User has no company assigned");

        if (!string.IsNullOrEmpty(companyId))
            return user.Companies.FirstOrDefault(x => x.Id == companyId);

        return user.Companies.FirstOrDefault(x => x.IsActive);
    }

    private async Task SetActiveCompanyAsync(User user, UserCompany selected)
    {
        user.LastLogin = DateTime.UtcNow;
        await _userRepository.SetActiveCompanyAsync(user.Id, selected.Id);
    }

    private async Task<UserDto> BuildUserResponseAsync(User user, UserCompany userCompany, string? deviceId = null, string? ipAddress = null, string? userAgent = null)
    {
        var token = _tokenService.CreateAccessToken(user.Id, userCompany.Id);

        var existing = await _refreshTokenRepository.GetActiveTokenAsync(
            user.Id, userCompany.Id, deviceId, ipAddress, userAgent);

        (string rawToken, RefreshToken entity) refreshToken;
        refreshToken = await _tokenService.CreateRefreshToken(user.Id, userCompany.Id, deviceId, ipAddress, userAgent);

        if (existing != null)
            await _refreshTokenRepository.RevokeAsync(existing, refreshToken.entity.Id);


        await _refreshTokenRepository.AddAsync(refreshToken.entity);

        var userDto = _mapper.Map<UserDto>(user);
        userDto.ActiveCompany = _mapper.Map<CompanyDto>(userCompany);
        userDto.Token = token;
        userDto.RefreshToken = refreshToken.rawToken;
        return userDto;
    }

    private async Task CheckUserExistsAsync(RegisterDto dto)
    {
        var existing = (await _userRepository.FindAsync(
            x => x.Email == dto.Email ||
                 x.UserName == dto.UserName ||
                 x.PhoneNumber == dto.PhoneNumber
        )).ToList();

        if (existing.Any())
            throw new Exception("User with the same email, username or phone number already exists");
    }

    private async Task<Invite?> ResolveInviteAsync(string? token)
    {
        if (string.IsNullOrEmpty(token)) return null;

        var invite = await _inviteRepository.FindOneAsync(
            x => x.Token == token &&
                 x.IsUsed == false &&
                 x.ExpireAt > DateTime.UtcNow
        );

        return invite;
    }
    public async Task<ReturnObject<UserDto>?> RefreshAsync(string? refreshToken)
    {
        if (refreshToken == null) return null;
        var refreshTokenEntity = await _refreshTokenRepository.GetByRawTokenAsync(refreshToken!);

        if (refreshTokenEntity == null)
            return null;

        if (refreshTokenEntity?.ExpiresAt < DateTime.UtcNow)
            return null;

        if (refreshTokenEntity?.RevokedAt != null)
            return null;

        var user = await _userRepository.FindOneAsync(x => x.Id == refreshTokenEntity!.UserId);
        if (user == null)
            throw new Exception("User not found");

        var userCompany = user?.Companies?.FirstOrDefault(x => x.Id == refreshTokenEntity?.CompanyId);
        if (userCompany == null)
            throw new Exception("Company not found");

        var newRefreshToken = await _tokenService.CreateRefreshToken(user?.Id!, userCompany.Id);
        await _refreshTokenRepository.RevokeAsync(refreshTokenEntity!, newRefreshToken.entity.Id);

        await _refreshTokenRepository.AddAsync(newRefreshToken.entity);
        var token = _tokenService.CreateAccessToken(user?.Id!, userCompany.Id);

        var userDto = _mapper.Map<UserDto>(user);
        userDto.ActiveCompany = _mapper.Map<CompanyDto>(userCompany);
        userDto.Token = token;
        userDto.RefreshToken = newRefreshToken.rawToken;

        return ReturnObject<UserDto>.Success(userDto);
    }

    public async Task<ReturnObject<UserDto>?> ValidateRefreshTokenAsync(string rawToken)
    {
        var token = await _refreshTokenRepository.GetByRawTokenAsync(rawToken);

        if (token == null || token.RevokedAt != null || token.ExpiresAt < DateTime.UtcNow)
            return null;

        var user = await _userRepository.GetByIdAsync(token.UserId);
        var userCompany = user?.Companies?.FirstOrDefault(x => x.Id == token.CompanyId && x.IsActive);

        if (user == null || userCompany == null) return null;

        var accessToken = _tokenService.CreateAccessToken(user.Id, userCompany.Id);

        var userDto = _mapper.Map<UserDto>(user);
        userDto.ActiveCompany = _mapper.Map<CompanyDto>(userCompany);
        userDto.Token = accessToken;

        return ReturnObject<UserDto>.Success(userDto, message: "Token Bilgisi Geçerli");
    }
    public async Task<ReturnObject<bool>> LogoutAsync(string? rawToken, string userId, LogoutDto? dto = null)
    {
        if (dto?.DeviceId != null)
        {
            var token = await _refreshTokenRepository.GetActiveTokenAsync(
                userId, dto.CompanyId!, dto.DeviceId);
            if (token != null)
                await _refreshTokenRepository.RevokeAsync(token);
            return ReturnObject<bool>.Success(true);
        }

        // Remote logout - tüm cihazlar
        if (dto?.AllDevices == true)
        {
            await _refreshTokenRepository.RevokeAllForUserAsync(userId);
            return ReturnObject<bool>.Success(true);
        }

        // Normal logout - mevcut session
        if (rawToken == null) return ReturnObject<bool>.Success(true);
        var refreshToken = await _refreshTokenRepository.GetByRawTokenAsync(rawToken);
        if (refreshToken == null || refreshToken.RevokedAt != null)
            return ReturnObject<bool>.Success(true);
        if (refreshToken.UserId != userId)
            throw new UnauthorizedAccessException("Token does not belong to this user");

        await _refreshTokenRepository.RevokeAsync(refreshToken);
        return ReturnObject<bool>.Success(true);
    }

}