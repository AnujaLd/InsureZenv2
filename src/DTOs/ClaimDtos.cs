namespace InsureZenv2.src.DTOs;

public class ClaimIngestDto
{
    public string PatientName { get; set; } = string.Empty;
    public string PatientId { get; set; } = string.Empty;
    public DateTime ServiceDate { get; set; }
    public decimal ClaimAmount { get; set; }
    public string ServiceDescription { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public string ProviderCode { get; set; } = string.Empty;
}

public class ClaimResponseDto
{
    public Guid Id { get; set; }
    public string ClaimNumber { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public string PatientId { get; set; } = string.Empty;
    public DateTime ServiceDate { get; set; }
    public decimal ClaimAmount { get; set; }
    public string ServiceDescription { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public string ProviderCode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class ClaimDetailDto : ClaimResponseDto
{
    public ClaimReviewDto? MakerReview { get; set; }
    public ClaimReviewDto? CheckerReview { get; set; }
}

public class ClaimReviewDto
{
    public Guid Id { get; set; }
    public string ReviewedByUsername { get; set; } = string.Empty;
    public string Feedback { get; set; } = string.Empty;
    public string? MakerRecommendation { get; set; }
    public string? CheckerDecision { get; set; }
    public DateTime ReviewedAt { get; set; }
}

public class MakerReviewSubmitDto
{
    public string Feedback { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty; // "Approve" or "Reject"
}

public class CheckerReviewSubmitDto
{
    public string Feedback { get; set; } = string.Empty;
    public string Decision { get; set; } = string.Empty; // "Approved" or "Rejected"
}

public class PaginatedClaimsDto
{
    public List<ClaimResponseDto> Claims { get; set; } = new List<ClaimResponseDto>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
