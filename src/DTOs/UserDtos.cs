namespace InsureZenv2.src.DTOs;

public class UserLoginDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class UserRegisterDto
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty; // "Maker" or "Checker"
    public Guid InsuranceCompanyId { get; set; }
}

public class UserResponseDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public UserResponseDto User { get; set; } = new();
    public int ExpiresIn { get; set; } // in seconds
}
