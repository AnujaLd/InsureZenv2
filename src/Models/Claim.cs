namespace InsureZenv2.src.Models;

/// <summary>
/// Represents a claim submitted for insurance review
/// </summary>
public class Claim
{
    public Guid Id { get; set; }
    public string ClaimNumber { get; set; } = string.Empty;
    public Guid InsuranceCompanyId { get; set; }
    
    // Extracted claim data (from upstream OCR/parsing service)
    public string PatientName { get; set; } = string.Empty;
    public string PatientId { get; set; } = string.Empty;
    public DateTime ServiceDate { get; set; }
    public decimal ClaimAmount { get; set; }
    public string ServiceDescription { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public string ProviderCode { get; set; } = string.Empty;
    
    // Claim status and workflow
    public ClaimStatus Status { get; set; } = ClaimStatus.Pending;
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    
    // Concurrency handling
    public Guid? LockedByUserId { get; set; }
    public DateTime? LockedAt { get; set; }
    
    // For audit trail
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public InsuranceCompany? InsuranceCompany { get; set; }
    public User? LockedByUser { get; set; }
    public ICollection<ClaimReview> MakerReview { get; set; } = new List<ClaimReview>();
}
