using AuthService.DTOs;
using AuthService.Models;
using AuthService.Repositories;
using AutoMapper;
using MongoDB.Bson;
using MongoDB.Driver;
using Shared.Enums;
using Shared.Models;
using System.Security.Cryptography;

namespace AuthService.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IInviteRepository _inviteRepository;
    private readonly ICompanyService _companyService;
    private readonly Shared.Services.TokenService _tokenService;
    private readonly IMapper _mapper;
    private readonly IConfiguration _config;

    public UserService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IInviteRepository inviteRepository,
        ICompanyService companyService,
        Shared.Services.TokenService tokenService,
        IMapper mapper,
        IConfiguration config)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _inviteRepository = inviteRepository;
        _companyService = companyService;
        _tokenService = tokenService;
        _mapper = mapper;
        _config = config;
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

    public async Task<ReturnObject<UserDto>?> RegisterAsync(UserInsertDto dto)
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
        refreshToken =  CreateRefreshToken(user.Id, userCompany.Id, deviceId, ipAddress, userAgent);

        if (existing != null)
            await _refreshTokenRepository.RevokeAsync(existing, refreshToken.entity.Id);


        await _refreshTokenRepository.AddAsync(refreshToken.entity);

        var userDto = _mapper.Map<UserDto>(user);
        userDto.ActiveCompany = _mapper.Map<CompanyDto>(userCompany);
        userDto.Token = token;
        userDto.RefreshToken = refreshToken.rawToken;
        return userDto;
    }

    private async Task CheckUserExistsAsync(UserInsertDto dto)
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
    public async Task<ReturnObject<UserDto>?> RefreshAsync(string? refreshToken, string? deviceId, string? ipAddress, string? userAgent)
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

        var newRefreshToken = CreateRefreshToken(user.Id, userCompany.Id, deviceId, ipAddress, userAgent);
        await _refreshTokenRepository.RevokeAsync(refreshTokenEntity!, newRefreshToken.entity.Id);

        await _refreshTokenRepository.AddAsync(newRefreshToken.entity);
        var token = _tokenService.CreateAccessToken(user?.Id!, userCompany.Id);

        var userDto = _mapper.Map<UserDto>(user);
        userDto.ActiveCompany = _mapper.Map<CompanyDto>(userCompany);
        userDto.Token = token;
        userDto.RefreshToken = newRefreshToken.rawToken;

        return ReturnObject<UserDto>.Success(userDto);
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

    public async Task<ReturnObject<UserDto>> GetProfileAsync(string userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) throw new Exception("User not found");

        var userDto = _mapper.Map<UserDto>(user);
        userDto.ActiveCompany = _mapper.Map<CompanyDto>(
            user.Companies?.FirstOrDefault(x => x.IsActive));

        return ReturnObject<UserDto>.Success(userDto);
    }

    public async Task<ReturnObject<UserSummaryDto>> GetUserSummaryAsync(string userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) throw new Exception("User not found");

        var userDto = _mapper.Map<UserSummaryDto>(user);
        userDto.ActiveCompany = _mapper.Map<CompanyDto>(
            user.Companies?.FirstOrDefault(x => x.IsActive));

        return ReturnObject<UserSummaryDto>.Success(userDto);
    }

    public async Task<ReturnObject<UserDto>> UpdateProfileAsync(string userId, UserUpdateDto dto)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) throw new Exception("User not found");

        if (!string.IsNullOrEmpty(dto.PhoneNumber) && dto.PhoneNumber != user.PhoneNumber)
        {
            var existing = await _userRepository.FindOneAsync(
                x => x.PhoneNumber == dto.PhoneNumber && x.Id != userId);
            if (existing != null) throw new Exception("Phone number already in use");
        }

        if (!string.IsNullOrEmpty(dto.Name)) user.Name  = dto.Name;
        if (!string.IsNullOrEmpty(dto.Surname)) user.Surname   = dto.Surname;
        if (!string.IsNullOrEmpty(dto.PhoneNumber)) user.PhoneNumber = dto.PhoneNumber;

        await _userRepository.UpdateAsync(user);

        var userDto = _mapper.Map<UserDto>(user);
        userDto.ActiveCompany = _mapper.Map<CompanyDto>(
            user.Companies?.FirstOrDefault(x => x.IsActive));

        return ReturnObject<UserDto>.Success(userDto);
    }

    public async Task<ReturnObject<bool>> ChangePasswordAsync(string userId, UserChangePasswordDto dto)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) throw new Exception("User not found");

        if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.Password))
            throw new Exception("Current password is incorrect");
        if (BCrypt.Net.BCrypt.Verify(dto.NewPassword, user.Password))
            throw new Exception("The new password cannot be the same as the old password.");

        user.Password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        await _userRepository.UpdateAsync(user);

        return ReturnObject<bool>.Success(true);
    }


    public async Task<User?> GetUserEntityAsync(string id)
    {
        User? result = await _userRepository.GetByIdAsync(id);
        if (result == null) return null;
        return result;
    }


    public async Task<ReturnObject<UserDto>?> GetByIdAsync(string id)
    {
        User? user = await _userRepository.GetByIdAsync(id);
        if (user == null) return null;
        UserDto result = _mapper.Map<UserDto>(user);
        return ReturnObject<UserDto>.Success(result);
    }
    public async Task RevokeAllForUserAsync(string? userId, string? deviceId, string? ipAdress, string? userAgent)
    {
        if (userId == null) return;
        await _refreshTokenRepository.RevokeAllForUserAsync(userId,deviceId,ipAdress,userAgent);
    }
    private (string rawToken, RefreshToken entity) CreateRefreshToken(
    string userId, string companyId,
    string? deviceId, string? ipAddress, string? userAgent)
    {
        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var entity = new RefreshToken
        {
            UserId    = userId,
            CompanyId = companyId,
            TokenHash = Shared.Services.TokenService.HashToken(rawToken),
            ExpiresAt = DateTime.UtcNow.AddDays(
                _config.GetValue<int>("Jwt:RefreshTokenDays", 30)),
            CreatedAt  = DateTime.UtcNow,
            DeviceId   = deviceId,
            IpAddress  = ipAddress,
            UserAgent  = userAgent
        };
        return (rawToken, entity);
    }

}