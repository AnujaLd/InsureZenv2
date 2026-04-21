using InsureZenv2.src.Authentication;
using InsureZenv2.src.DTOs;
using InsureZenv2.src.Models;
using InsureZenv2.src.Repositories;

namespace InsureZenv2.src.Services;

public interface IAuthenticationService
{
    Task<AuthResponseDto?> LoginAsync(UserLoginDto dto);
    Task<UserResponseDto?> RegisterAsync(UserRegisterDto dto);
}

public class AuthenticationService : IAuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _tokenService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(
        IUserRepository userRepository,
        IJwtTokenService tokenService,
        IPasswordHasher passwordHasher,
        ILogger<AuthenticationService> logger)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<AuthResponseDto?> LoginAsync(UserLoginDto dto)
    {
        var user = await _userRepository.GetByUsernameAsync(dto.Username);
        if (user == null || !user.IsActive)
        {
            _logger.LogWarning($"Login attempt failed for user {dto.Username}");
            return null;
        }

        if (!_passwordHasher.VerifyPassword(dto.Password, user.PasswordHash))
        {
            _logger.LogWarning($"Invalid password for user {dto.Username}");
            return null;
        }

        var token = _tokenService.GenerateToken(user);
        _logger.LogInformation($"User {user.Username} logged in successfully");

        return new AuthResponseDto
        {
            Token = token,
            User = new UserResponseDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                IsActive = user.IsActive
            },
            ExpiresIn = 3600 // 1 hour
        };
    }

    public async Task<UserResponseDto?> RegisterAsync(UserRegisterDto dto)
    {
        var existingUser = await _userRepository.GetByUsernameAsync(dto.Username);
        if (existingUser != null)
        {
            _logger.LogWarning($"Registration attempt with existing username: {dto.Username}");
            return null;
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = _passwordHasher.HashPassword(dto.Password),
            Role = dto.Role,
            InsuranceCompanyId = dto.InsuranceCompanyId,
            IsActive = true
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        _logger.LogInformation($"User {user.Username} registered successfully");

        return new UserResponseDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            IsActive = user.IsActive
        };
    }
}
