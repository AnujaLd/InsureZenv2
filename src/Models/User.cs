namespace InsureZenv2.src.Models;

/// <summary>
/// Represents a user (Maker or Checker)
/// </summary>
public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty; // "Maker" or "Checker"
    public Guid InsuranceCompanyId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public InsuranceCompany? InsuranceCompany { get; set; }
    public ICollection<ClaimReview> ReviewedClaims { get; set; } = new List<ClaimReview>();
}
