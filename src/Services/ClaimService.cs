using AutoMapper;
using InsureZenv2.src.DTOs;
using InsureZenv2.src.Models;
using InsureZenv2.src.Repositories;

namespace InsureZenv2.src.Services;

public interface IClaimService
{
    Task<ClaimResponseDto> IngestClaimAsync(ClaimIngestDto dto, Guid insuranceCompanyId);
    Task<ClaimDetailDto?> GetClaimByIdAsync(Guid claimId);
    Task<PaginatedClaimsDto> GetClaimsForMakerAsync(Guid userId, int pageNumber = 1, int pageSize = 10, 
        string? status = null, Guid? insuranceCompanyId = null, DateTime? fromDate = null, DateTime? toDate = null);
    Task<PaginatedClaimsDto> GetClaimsForCheckerAsync(Guid userId, int pageNumber = 1, int pageSize = 10,
        string? status = null, Guid? insuranceCompanyId = null, DateTime? fromDate = null, DateTime? toDate = null);
    Task<bool> LockClaimForMakerAsync(Guid claimId, Guid userId);
    Task<bool> LockClaimForCheckerAsync(Guid claimId, Guid userId);
    Task<bool> UnlockClaimAsync(Guid claimId, Guid userId);
    Task<ClaimDetailDto?> SubmitMakerReviewAsync(Guid claimId, Guid userId, MakerReviewSubmitDto dto);
    Task<ClaimDetailDto?> SubmitCheckerDecisionAsync(Guid claimId, Guid userId, CheckerReviewSubmitDto dto);
    Task<bool> ForwardToInsurerAsync(Guid claimId);
}

public class ClaimService : IClaimService
{
    private readonly IClaimRepository _claimRepository;
    private readonly IClaimReviewRepository _reviewRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ClaimService> _logger;

    public ClaimService(
        IClaimRepository claimRepository,
        IClaimReviewRepository reviewRepository,
        IAuditLogRepository auditLogRepository,
        IMapper mapper,
        ILogger<ClaimService> logger)
    {
        _claimRepository = claimRepository;
        _reviewRepository = reviewRepository;
        _auditLogRepository = auditLogRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ClaimResponseDto> IngestClaimAsync(ClaimIngestDto dto, Guid insuranceCompanyId)
    {
        var claim = _mapper.Map<Claim>(dto);
        claim.InsuranceCompanyId = insuranceCompanyId;

        await _claimRepository.AddAsync(claim);
        await _claimRepository.SaveChangesAsync();

        _logger.LogInformation($"Claim {claim.ClaimNumber} ingested");

        return _mapper.Map<ClaimResponseDto>(claim);
    }

    public async Task<ClaimDetailDto?> GetClaimByIdAsync(Guid claimId)
    {
        var claim = await _claimRepository.GetByIdAsync(claimId);
        if (claim == null)
            return null;

        return _mapper.Map<ClaimDetailDto>(claim);
    }

    public async Task<PaginatedClaimsDto> GetClaimsForMakerAsync(Guid userId, int pageNumber = 1, int pageSize = 10,
        string? status = null, Guid? insuranceCompanyId = null, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var claims = await _claimRepository.GetClaimsForMakerAsync(pageNumber, pageSize, status, insuranceCompanyId, fromDate, toDate);
        var totalCount = await _claimRepository.GetTotalCountAsync(status, insuranceCompanyId, fromDate, toDate);

        var claimDtos = _mapper.Map<List<ClaimResponseDto>>(claims);

        return new PaginatedClaimsDto
        {
            Claims = claimDtos,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };
    }

    public async Task<PaginatedClaimsDto> GetClaimsForCheckerAsync(Guid userId, int pageNumber = 1, int pageSize = 10,
        string? status = null, Guid? insuranceCompanyId = null, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var claims = await _claimRepository.GetClaimsForCheckerAsync(pageNumber, pageSize, status, insuranceCompanyId, fromDate, toDate);
        var totalCount = await _claimRepository.GetTotalCountAsync(status, insuranceCompanyId, fromDate, toDate);

        var claimDtos = _mapper.Map<List<ClaimResponseDto>>(claims);

        return new PaginatedClaimsDto
        {
            Claims = claimDtos,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };
    }

    public async Task<bool> LockClaimForMakerAsync(Guid claimId, Guid userId)
    {
        var result = await _claimRepository.LockClaimAsync(claimId, userId, "Maker");
        if (result != null)
        {
            await LogAuditAsync(claimId, userId, "CLAIM_LOCKED_BY_MAKER", "Maker locked the claim for review");
            return true;
        }
        return false;
    }

    public async Task<bool> LockClaimForCheckerAsync(Guid claimId, Guid userId)
    {
        var result = await _claimRepository.LockClaimAsync(claimId, userId, "Checker");
        if (result != null)
        {
            await LogAuditAsync(claimId, userId, "CLAIM_LOCKED_BY_CHECKER", "Checker locked the claim for review");
            return true;
        }
        return false;
    }

    public async Task<bool> UnlockClaimAsync(Guid claimId, Guid userId)
    {
        var result = await _claimRepository.UnlockClaimAsync(claimId, userId);
        if (result)
        {
            await LogAuditAsync(claimId, userId, "CLAIM_UNLOCKED", "Claim unlocked");
            return true;
        }
        return false;
    }

    public async Task<ClaimDetailDto?> SubmitMakerReviewAsync(Guid claimId, Guid userId, MakerReviewSubmitDto dto)
    {
        var claim = await _claimRepository.GetByIdAsync(claimId);
        if (claim == null || claim.LockedByUserId != userId)
            return null;

        var recommendation = dto.Recommendation == "Approve" 
            ? MakerRecommendation.Approve 
            : MakerRecommendation.Reject;

        var review = new ClaimReview
        {
            Id = Guid.NewGuid(),
            ClaimId = claimId,
            ReviewedByUserId = userId,
            IsMakerReview = true,
            Feedback = dto.Feedback,
            MakerRecommendation = recommendation
        };

        claim.Status = ClaimStatus.MakerSubmitted;
        claim.LockedByUserId = null;
        claim.LockedAt = null;

        await _reviewRepository.AddAsync(review);
        await _claimRepository.UpdateAsync(claim);
        await _claimRepository.SaveChangesAsync();

        await LogAuditAsync(claimId, userId, "MAKER_REVIEW_SUBMITTED", 
            $"Maker submitted review with recommendation: {recommendation}");

        _logger.LogInformation($"Maker review submitted for claim {claim.ClaimNumber}");

        return _mapper.Map<ClaimDetailDto>(claim);
    }

    public async Task<ClaimDetailDto?> SubmitCheckerDecisionAsync(Guid claimId, Guid userId, CheckerReviewSubmitDto dto)
    {
        var claim = await _claimRepository.GetByIdAsync(claimId);
        if (claim == null || claim.LockedByUserId != userId)
            return null;

        var decision = dto.Decision == "Approved" 
            ? CheckerDecision.Approved 
            : CheckerDecision.Rejected;

        var review = new ClaimReview
        {
            Id = Guid.NewGuid(),
            ClaimId = claimId,
            ReviewedByUserId = userId,
            IsMakerReview = false,
            Feedback = dto.Feedback,
            CheckerDecision = decision
        };

        claim.Status = decision == CheckerDecision.Approved ? ClaimStatus.Approved : ClaimStatus.Rejected;
        claim.LockedByUserId = null;
        claim.LockedAt = null;
        claim.CompletedAt = DateTime.UtcNow;

        await _reviewRepository.AddAsync(review);
        await _claimRepository.UpdateAsync(claim);
        await _claimRepository.SaveChangesAsync();

        await LogAuditAsync(claimId, userId, "CHECKER_DECISION_SUBMITTED", 
            $"Checker submitted decision: {decision}");

        _logger.LogInformation($"Checker decision submitted for claim {claim.ClaimNumber}");

        return _mapper.Map<ClaimDetailDto>(claim);
    }

    public async Task<bool> ForwardToInsurerAsync(Guid claimId)
    {
        var claim = await _claimRepository.GetByIdAsync(claimId);
        if (claim == null || (claim.Status != ClaimStatus.Approved && claim.Status != ClaimStatus.Rejected))
            return false;

        claim.Status = ClaimStatus.ForwardedToInsurer;
        await _claimRepository.UpdateAsync(claim);
        await _claimRepository.SaveChangesAsync();

        await LogAuditAsync(claimId, null, "CLAIM_FORWARDED_TO_INSURER", 
            "Claim forwarded to insurance company");

        _logger.LogInformation($"Claim {claim.ClaimNumber} forwarded to insurer");

        return true;
    }

    private async Task LogAuditAsync(Guid claimId, Guid? userId, string action, string details)
    {
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            ClaimId = claimId,
            UserId = userId,
            Action = action,
            Details = details
        };

        await _auditLogRepository.AddAsync(auditLog);
        await _auditLogRepository.SaveChangesAsync();
    }
}
