using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using InsureZenv2.src.DTOs;
using InsureZenv2.src.Services;
using FluentValidation;

namespace InsureZenv2.src.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authService;
    private readonly IValidator<UserLoginDto> _loginValidator;
    private readonly IValidator<UserRegisterDto> _registerValidator;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthenticationService authService,
        IValidator<UserLoginDto> loginValidator,
        IValidator<UserRegisterDto> registerValidator,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _loginValidator = loginValidator;
        _registerValidator = registerValidator;
        _logger = logger;
    }

    /// <summary>
    /// User login endpoint
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] UserLoginDto dto)
    {
        var validationResult = await _loginValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });
        }

        var result = await _authService.LoginAsync(dto);
        if (result == null)
        {
            return Unauthorized(new { message = "Invalid username or password" });
        }

        return Ok(result);
    }

    /// <summary>
    /// User registration endpoint
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] UserRegisterDto dto)
    {
        var validationResult = await _registerValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });
        }

        var result = await _authService.RegisterAsync(dto);
        if (result == null)
        {
            return Conflict(new { message = "User already exists" });
        }

        return CreatedAtAction(nameof(Login), new { username = result.Username }, result);
    }
}
