using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using InsureZenv2.src.DTOs;
using InsureZenv2.src.Services;
using FluentValidation;

namespace InsureZenv2.src.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ClaimsController : ControllerBase
{
    private readonly IClaimService _claimService;
    private readonly IValidator<ClaimIngestDto> _ingestValidator;
    private readonly IValidator<MakerReviewSubmitDto> _makerReviewValidator;
    private readonly IValidator<CheckerReviewSubmitDto> _checkerReviewValidator;
    private readonly ILogger<ClaimsController> _logger;

    public ClaimsController(
        IClaimService claimService,
        IValidator<ClaimIngestDto> ingestValidator,
        IValidator<MakerReviewSubmitDto> makerReviewValidator,
        IValidator<CheckerReviewSubmitDto> checkerReviewValidator,
        ILogger<ClaimsController> logger)
    {
        _claimService = claimService;
        _ingestValidator = ingestValidator;
        _makerReviewValidator = makerReviewValidator;
        _checkerReviewValidator = checkerReviewValidator;
        _logger = logger;
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        return Guid.Parse(userIdClaim?.Value ?? Guid.Empty.ToString());
    }

    private string GetUserRole()
    {
        return User.FindFirst("Role")?.Value ?? string.Empty;
    }

    private Guid GetInsuranceCompanyId()
    {
        var companyIdClaim = User.FindFirst("InsuranceCompanyId");
        return Guid.Parse(companyIdClaim?.Value ?? Guid.Empty.ToString());
    }

    /// <summary>
    /// Ingest a new claim
    /// </summary>
    [HttpPost("ingest")]
    public async Task<IActionResult> IngestClaim([FromBody] ClaimIngestDto dto)
    {
        var validationResult = await _ingestValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });
        }

        try
        {
            var result = await _claimService.IngestClaimAsync(dto, GetInsuranceCompanyId());
            return CreatedAtAction(nameof(GetClaimById), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error ingesting claim: {ex.Message}");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get claim by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetClaimById(Guid id)
    {
        try
        {
            var claim = await _claimService.GetClaimByIdAsync(id);
            if (claim == null)
            {
                return NotFound(new { message = "Claim not found" });
            }
            return Ok(claim);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving claim: {ex.Message}");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get claims for maker review (paginated)
    /// </summary>
    [HttpGet("maker/list")]
    [Authorize(Policy = "MakerOnly")]
    public async Task<IActionResult> GetClaimsForMaker(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null,
        [FromQuery] Guid? insuranceCompanyId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var result = await _claimService.GetClaimsForMakerAsync(GetUserId(), pageNumber, pageSize, status, insuranceCompanyId, fromDate, toDate);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving claims for maker: {ex.Message}");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get claims for checker review (paginated)
    /// </summary>
    [HttpGet("checker/list")]
    [Authorize(Policy = "CheckerOnly")]
    public async Task<IActionResult> GetClaimsForChecker(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null,
        [FromQuery] Guid? insuranceCompanyId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var result = await _claimService.GetClaimsForCheckerAsync(GetUserId(), pageNumber, pageSize, status, insuranceCompanyId, fromDate, toDate);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving claims for checker: {ex.Message}");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Lock claim for maker review
    /// </summary>
    [HttpPost("{id}/lock/maker")]
    [Authorize(Policy = "MakerOnly")]
    public async Task<IActionResult> LockClaimForMaker(Guid id)
    {
        try
        {
            var result = await _claimService.LockClaimForMakerAsync(id, GetUserId());
            if (!result)
            {
                return BadRequest(new { message = "Cannot lock claim. It may be locked by another user or not in the correct status." });
            }
            return Ok(new { message = "Claim locked successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error locking claim for maker: {ex.Message}");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Lock claim for checker review
    /// </summary>
    [HttpPost("{id}/lock/checker")]
    [Authorize(Policy = "CheckerOnly")]
    public async Task<IActionResult> LockClaimForChecker(Guid id)
    {
        try
        {
            var result = await _claimService.LockClaimForCheckerAsync(id, GetUserId());
            if (!result)
            {
                return BadRequest(new { message = "Cannot lock claim. It may be locked by another user or not in the correct status." });
            }
            return Ok(new { message = "Claim locked successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error locking claim for checker: {ex.Message}");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Unlock claim
    /// </summary>
    [HttpPost("{id}/unlock")]
    public async Task<IActionResult> UnlockClaim(Guid id)
    {
        try
        {
            var result = await _claimService.UnlockClaimAsync(id, GetUserId());
            if (!result)
            {
                return BadRequest(new { message = "Cannot unlock claim. You don't have permission." });
            }
            return Ok(new { message = "Claim unlocked successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error unlocking claim: {ex.Message}");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Submit maker review
    /// </summary>
    [HttpPost("{id}/review/maker")]
    [Authorize(Policy = "MakerOnly")]
    public async Task<IActionResult> SubmitMakerReview(Guid id, [FromBody] MakerReviewSubmitDto dto)
    {
        var validationResult = await _makerReviewValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });
        }

        try
        {
            var result = await _claimService.SubmitMakerReviewAsync(id, GetUserId(), dto);
            if (result == null)
            {
                return BadRequest(new { message = "Cannot submit review. You don't have permission or claim is not locked by you." });
            }
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error submitting maker review: {ex.Message}");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Submit checker decision
    /// </summary>
    [HttpPost("{id}/review/checker")]
    [Authorize(Policy = "CheckerOnly")]
    public async Task<IActionResult> SubmitCheckerDecision(Guid id, [FromBody] CheckerReviewSubmitDto dto)
    {
        var validationResult = await _checkerReviewValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });
        }

        try
        {
            var result = await _claimService.SubmitCheckerDecisionAsync(id, GetUserId(), dto);
            if (result == null)
            {
                return BadRequest(new { message = "Cannot submit decision. You don't have permission or claim is not locked by you." });
            }
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error submitting checker decision: {ex.Message}");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Forward completed claim to insurer
    /// </summary>
    [HttpPost("{id}/forward-to-insurer")]
    public async Task<IActionResult> ForwardToInsurer(Guid id)
    {
        try
        {
            var result = await _claimService.ForwardToInsurerAsync(id);
            if (!result)
            {
                return BadRequest(new { message = "Claim cannot be forwarded. It must be in Approved or Rejected status." });
            }
            return Ok(new { message = "Claim forwarded to insurer successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error forwarding claim to insurer: {ex.Message}");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}
