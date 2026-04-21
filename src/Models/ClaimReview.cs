namespace InsureZenv2.src.Models;

/// <summary>
/// Represents a review (by Maker or Checker) of a claim
/// </summary>
public class ClaimReview
{
    public Guid Id { get; set; }
    public Guid ClaimId { get; set; }
    public Guid ReviewedByUserId { get; set; }
    
    // Review type
    public bool IsMakerReview { get; set; } = true; // true for Maker, false for Checker
    
    // Review content
    public string Feedback { get; set; } = string.Empty;
    public MakerRecommendation? MakerRecommendation { get; set; }
    public CheckerDecision? CheckerDecision { get; set; }
    
    // Audit trail
    public DateTime ReviewedAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public Claim? Claim { get; set; }
    public User? ReviewedByUser { get; set; }
}
