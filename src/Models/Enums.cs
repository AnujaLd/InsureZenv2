namespace InsureZenv2.src.Models;

/// <summary>
/// Represents claim status states
/// </summary>
public enum ClaimStatus
{
    Pending = 0,           // Initial state when claim is ingested
    MakerInProgress = 1,   // Claim is being reviewed by a Maker
    MakerSubmitted = 2,    // Maker has submitted recommendation
    CheckerInProgress = 3, // Checker is reviewing
    Approved = 4,          // Final approval by Checker
    Rejected = 5,          // Final rejection by Checker
    ForwardedToInsurer = 6 // Completed, forwarded to insurance company
}

/// <summary>
/// Represents the recommendation made by a Maker
/// </summary>
public enum MakerRecommendation
{
    Pending = 0,
    Approve = 1,
    Reject = 2
}

/// <summary>
/// Represents the final decision made by a Checker
/// </summary>
public enum CheckerDecision
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}
