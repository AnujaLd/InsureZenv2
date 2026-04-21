namespace InsureZenv2.src.Models;

/// <summary>
/// Represents an insurance company using InsureZen services
/// </summary>
public class InsuranceCompany
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<Claim> Claims { get; set; } = new List<Claim>();
    public ICollection<User> Users { get; set; } = new List<User>();
}
