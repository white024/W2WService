// AuthService/Data/Services/UserService.cs
using AuthService.Data.Repositories.Interfaces;
using AuthService.Data.Services.Interfaces;
using AuthService.Models.DTOs;
using AuthService.Models.Entities;
using AutoMapper;
using Org.BouncyCastle.Crypto.Generators;
using Shared.Contracts.Events.User;
using Shared.Enums;
using Shared.Grpc;
using Shared.Kafka.Interfaces;
using Shared.Kafka.Topics;
using Shared.Models;
using Shared.Services;
using BCrypt.Net;
using System.Security.Cryptography;

namespace AuthService.Data.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly TokenService _tokenService;
    private readonly IKafkaProducer _kafkaProducer;
    private readonly CustomerGrpc.CustomerGrpcClient _customerGrpc;
    private readonly IConfiguration _config;
    private readonly ILogger<UserService> _logger;
    private readonly IMapper _mapper;

    public UserService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        TokenService tokenService,
        IKafkaProducer kafkaProducer,
        CustomerGrpc.CustomerGrpcClient customerGrpc,
        IConfiguration config,
        ILogger<UserService> logger, IMapper mapper)
    {
        _userRepository         = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _tokenService           = tokenService;
        _kafkaProducer          = kafkaProducer;
        _customerGrpc           = customerGrpc;
        _config                 = config;
        _logger                 = logger;
        _mapper                 = mapper;
    }


    public async Task<ReturnObject<UserDto>> LoginAsync(UserLoginDto dto, CancellationToken ct = default)
    {
        var user = await ResolveUserAsync(dto.UserName, dto.Password, dto.IpAddress, ct);

        List<CompanyItem> companies = [];
        try
        {
            var response = await _customerGrpc.GetUserCompaniesAsync(
                new GetUserCompaniesRequest { UserId = user.Id }, cancellationToken: ct);
            companies = [.. response.Companies];
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "CustomerService gRPC çağrısı başarısız, B2C olarak devam ediliyor");
        }

        if (companies.Count == 0)
        {
            var userDto = await BuildResponseAsync(user, null, null, dto, ct);
            return ReturnObject<UserDto>.Success(userDto);
        }

        if (dto?.CompanyId != null! && dto.CompanyId > 0)
        {
            var selected = companies.FirstOrDefault(x => x.Id == dto.CompanyId)
                ?? throw new Exception("Şirket bulunamadı");

            var userDto = await BuildResponseAsync(
                user, selected.Id, selected.CustomerId == 0 ? null : selected.CustomerId, dto, ct);
            return ReturnObject<UserDto>.Success(userDto);
        }

        if (companies.Count == 1)
        {
            var c = companies.First();
            var userDto = await BuildResponseAsync(
                user, c.Id, c.CustomerId == 0 ? null : c.CustomerId, dto, ct);
            return ReturnObject<UserDto>.Success(userDto);
        }

        return ReturnObject<UserDto>.SendResponse("Company selection required", companies);
    }


    public async Task<ReturnObject<UserDto>> RegisterAsync(UserCreateDto dto, CancellationToken ct = default)
    {
        if (await _userRepository.IsEmailTakenAsync(dto.Email, ct))
            throw new Exception("Bu email zaten kayıtlı");

        var user = new User
        {
            Email           = dto.Email,
            Name            = dto.Name,
            Surname         = dto.Surname,
            UserName        = dto.UserName ?? dto.Email,
            PhoneNumber     = dto.PhoneNumber,
            Password        = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            InviteCode      = dto.InviteCode,
            UserRole        = UserRole.Customer,
            Status          = UserStatus.Active,
            IsEmailVerified = false
        };

        var result = await _userRepository.AddAsync(user, ct);

        await _kafkaProducer.PublishAsync(
            KafkaTopics.UserTopics.Registered,
            result.Id,
            new UserRegisteredEvent
            {
                UserId      = result.Id,
                Email       = result.Email,
                Name        = result.Name,
                Surname     = result.Surname,
                InviteCode = dto.InviteCode
            }, ct);

        var userDto = await BuildResponseAsync(result, null, null,
            new UserLoginDto
            {
                DeviceId  = dto.DeviceId,
                IpAddress = dto.IpAddress,
                UserAgent = dto.UserAgent
            }, ct);

        return ReturnObject<UserDto>.Success(userDto);
    }


    public async Task<ReturnObject<UserDto>?> RefreshAsync(
        string refreshToken, string? deviceId, string? ipAddress, string? userAgent,
        CancellationToken ct = default)
    {
        RefreshToken? tokenEntity = await _refreshTokenRepository.GetByRawTokenAsync(refreshToken, ct);

        if (tokenEntity == null || !tokenEntity.IsActive)
            return null;

        var user = await _userRepository.GetByIdAsync(tokenEntity.UserId, ct);
        if (user == null) return null;

        var newToken = _tokenService.CreateAccessToken(
            user.Id, tokenEntity?.CompanyId!, tokenEntity?.CustomerId);

        var userDto = _mapper.Map<UserDto>(user);
        userDto.Token      = newToken;
        userDto.RefreshToken = refreshToken;
        userDto.CompanyId  = tokenEntity?.CompanyId;
        userDto.CustomerId = tokenEntity?.CustomerId;

        return ReturnObject<UserDto>.Success(userDto);
    }


    public async Task<ReturnObject<bool>> LogoutAsync(
        string rawToken, string userId, LogoutDto? dto = null,
        CancellationToken ct = default)
    {
        // Tüm cihazlar
        if (dto?.AllDevices == true)
        {
            await _refreshTokenRepository.RevokeAllForUserAsync(
                userId,
                reason: RevokeReason.Logout.ToString(),
                ct: ct);
            return ReturnObject<bool>.Success(true);
        }

        var token = await _refreshTokenRepository.GetByRawTokenAsync(rawToken, ct);
        if (token == null || token.IsRevoked)
            return ReturnObject<bool>.Success(true);

        if (token.UserId != userId)
            throw new UnauthorizedAccessException("Token bu kullanıcıya ait değil");

        await _refreshTokenRepository.RevokeAsync(
            token,
            ipAddress: dto?.IpAddress,
            deviceId: dto?.DeviceId,
            userAgent: dto?.UserAgent,
            reason: RevokeReason.Logout.ToString(),
            ct: ct);

        return ReturnObject<bool>.Success(true);
    }

    public async Task RevokeAllAsync(
        string? userId, string? ipAddress, string? deviceId, string? userAgent,
        CancellationToken ct = default)
    {
        if (userId == null) return;
        await _refreshTokenRepository.RevokeAllForUserAsync(
            userId, ipAddress, deviceId, userAgent,
            RevokeReason.AdminRevoke.ToString(), ct);
    }


    public async Task<ReturnObject<UserDto>> GetProfileAsync(string userId, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, ct)
            ?? throw new Exception("Kullanıcı bulunamadı");
        return ReturnObject<UserDto>.Success(_mapper.Map<UserDto>(user));
    }

    public async Task<ReturnObject<UserDto>> UpdateProfileAsync(
        string userId, UserUpdateDto dto, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, ct)
            ?? throw new Exception("Kullanıcı bulunamadı");

        if (!string.IsNullOrEmpty(dto.Name)) user.Name        = dto.Name;
        if (!string.IsNullOrEmpty(dto.Surname)) user.Surname     = dto.Surname;
        if (!string.IsNullOrEmpty(dto.PhoneNumber)) user.PhoneNumber = dto.PhoneNumber;

        await _userRepository.UpdateAsync(user, ct);
        return ReturnObject<UserDto>.Success(_mapper.Map<UserDto>(user));
    }

    public async Task<ReturnObject<bool>> ChangePasswordAsync(
        string userId, UserChangePasswordDto dto, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, ct)
            ?? throw new Exception("Kullanıcı bulunamadı");

        if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.Password))
            throw new Exception("Mevcut şifre hatalı");

        if (BCrypt.Net.BCrypt.Verify(dto.NewPassword, user.Password))
            throw new Exception("Yeni şifre eski şifre ile aynı olamaz");

        user.Password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        await _userRepository.UpdateAsync(user, ct);

        await _refreshTokenRepository.RevokeAllForUserAsync(
            userId,
            reason: RevokeReason.PasswordChange.ToString(),
            ct: ct);

        return ReturnObject<bool>.Success(true);
    }

    public async Task<ReturnObject<UserDto>?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(id, ct);
        if (user == null) return null;
        return ReturnObject<UserDto>.Success(_mapper.Map<UserDto>(user));
    }

    public async Task<User?> GetUserEntityAsync(string id, CancellationToken ct = default)
        => await _userRepository.GetByIdAsync(id, ct);


    private async Task<User> ResolveUserAsync(
        string userName, string password, string? ipAddress,
        CancellationToken ct)
    {
        var user = await _userRepository.GetByEmailAsync(userName, ct)
                ?? await _userRepository.GetByUserNameAsync(userName, ct)
                ?? await _userRepository.GetByPhoneAsync(userName, ct)
                ?? throw new Exception("Kullanıcı bulunamadı");

        if (user.IsLockedOut)
            throw new Exception($"Hesap kilitli. {user.LockoutEnd:HH:mm}'e kadar bekleyin");

        if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
        {
            user.IncrementFailedLogin();
            await _userRepository.UpdateAsync(user, ct);
            throw new Exception("Kullanıcı adı veya şifre hatalı");
        }

        user.ResetFailedLogin();
        user.SetLastLogin(ipAddress);
        await _userRepository.UpdateAsync(user, ct);

        return user;
    }

    private async Task<UserDto> BuildResponseAsync(
        User user, int? companyId, int? customerId,
        UserLoginDto dto, CancellationToken ct)
    {
        var rawRefreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        var tokenEntity = new RefreshToken
        {
            UserId     = user.Id,
            CompanyId  = companyId,
            CustomerId = customerId,
            TokenHash  = TokenService.HashToken(rawRefreshToken),
            ExpiresAt  = DateTime.UtcNow.AddDays(
                _config.GetValue<int>("Jwt:RefreshTokenDays", 30)),
            DeviceId   = dto.DeviceId,
            IpAddress  = dto.IpAddress,
            UserAgent  = dto.UserAgent
        };

        // Aynı cihazda aktif token varsa revoke et (rotation)
        var existing = await _refreshTokenRepository.GetActiveTokenAsync(
            user.Id, companyId, dto.DeviceId, ct);

        if (existing != null)
            await _refreshTokenRepository.RevokeAsync(
                existing,
                replacedById: tokenEntity.Id,
                ipAddress: dto.IpAddress,
                deviceId: dto.DeviceId,
                userAgent: dto.UserAgent,
                reason: RevokeReason.Refresh.ToString(),
                ct: ct);

        await _refreshTokenRepository.AddAsync(tokenEntity, ct);

        var accessToken = _tokenService.CreateAccessToken(
            user.Id,
            companyId,
            customerId);

        var userDto = _mapper.Map<UserDto>(user);
        userDto.Token      = accessToken;
        userDto.RefreshToken = rawRefreshToken;
        userDto.CompanyId  = companyId;
        userDto.CustomerId = customerId;
        return userDto;
    }


}