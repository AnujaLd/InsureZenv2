using FluentValidation;
using InsureZenv2.src.DTOs;
using InsureZenv2.src.Repositories;

namespace InsureZenv2.src.Validators;

public class UserLoginDtoValidator : AbstractValidator<UserLoginDto>
{
    public UserLoginDtoValidator()
    {
        RuleFor(u => u.Username)
            .NotEmpty().WithMessage("Username is required");

        RuleFor(u => u.Password)
            .NotEmpty().WithMessage("Password is required");
    }
}

public class UserRegisterDtoValidator : AbstractValidator<UserRegisterDto>
{
    private readonly IUserRepository _userRepository;

    public UserRegisterDtoValidator(IUserRepository userRepository)
    {
        _userRepository = userRepository;

        RuleFor(u => u.Username)
            .NotEmpty().WithMessage("Username is required")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters")
            .MaximumLength(255).WithMessage("Username cannot exceed 255 characters")
            .MustAsync(async (username, _) => await UniqueUsername(username))
            .WithMessage("Username already exists");

        RuleFor(u => u.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email must be valid")
            .MustAsync(async (email, _) => await UniqueEmail(email))
            .WithMessage("Email already exists");

        RuleFor(u => u.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one number")
            .Matches(@"[!@#$%^&*]").WithMessage("Password must contain at least one special character (!@#$%^&*)");

        RuleFor(u => u.Role)
            .NotEmpty().WithMessage("Role is required")
            .Must(r => r == "Maker" || r == "Checker")
            .WithMessage("Role must be either 'Maker' or 'Checker'");

        RuleFor(u => u.InsuranceCompanyId)
            .NotEmpty().WithMessage("Insurance company ID is required");
    }

    private async Task<bool> UniqueUsername(string username)
    {
        var user = await _userRepository.GetByUsernameAsync(username);
        return user == null;
    }

    private async Task<bool> UniqueEmail(string email)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        return user == null;
    }
}
