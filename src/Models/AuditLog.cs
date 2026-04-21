namespace InsureZenv2.src.Models;

/// <summary>
/// Audit log for claim state transitions
/// </summary>
public class AuditLog
{
    public Guid Id { get; set; }
    public Guid ClaimId { get; set; }
    public Guid? UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Claim? Claim { get; set; }
    public User? User { get; set; }
}
