using Microsoft.EntityFrameworkCore;
using InsureZenv2.src.Data;
using InsureZenv2.src.Models;

namespace InsureZenv2.src.Repositories;

public interface IClaimRepository
{
    Task<Claim?> GetByIdAsync(Guid id);
    Task<Claim?> GetByClaimNumberAsync(string claimNumber);
    Task<List<Claim>> GetClaimsForMakerAsync(int pageNumber = 1, int pageSize = 10, 
        string? status = null, Guid? insuranceCompanyId = null, DateTime? fromDate = null, DateTime? toDate = null);
    Task<List<Claim>> GetClaimsForCheckerAsync(int pageNumber = 1, int pageSize = 10, 
        string? status = null, Guid? insuranceCompanyId = null, DateTime? fromDate = null, DateTime? toDate = null);
    Task<int> GetTotalCountAsync(string? status = null, Guid? insuranceCompanyId = null, DateTime? fromDate = null, DateTime? toDate = null);
    Task<Claim?> LockClaimAsync(Guid claimId, Guid userId, string role);
    Task<bool> UnlockClaimAsync(Guid claimId, Guid userId);
    Task AddAsync(Claim claim);
    Task UpdateAsync(Claim claim);
    Task<bool> SaveChangesAsync();
}

public class ClaimRepository : IClaimRepository
{
    private readonly ApplicationDbContext _context;

    public ClaimRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Claim?> GetByIdAsync(Guid id)
    {
        return await _context.Claims
            .Include(c => c.InsuranceCompany)
            .Include(c => c.MakerReview)
                .ThenInclude(r => r!.ReviewedByUser)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Claim?> GetByClaimNumberAsync(string claimNumber)
    {
        return await _context.Claims
            .FirstOrDefaultAsync(c => c.ClaimNumber == claimNumber);
    }

    public async Task<List<Claim>> GetClaimsForMakerAsync(int pageNumber = 1, int pageSize = 10,
        string? status = null, Guid? insuranceCompanyId = null, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.Claims
            .Include(c => c.InsuranceCompany)
            .Include(c => c.MakerReview)
                .ThenInclude(r => r!.ReviewedByUser)
            .AsQueryable();

        // Maker can see Pending and MakerInProgress claims
        query = query.Where(c => c.Status == ClaimStatus.Pending || c.Status == ClaimStatus.MakerInProgress);

        if (!string.IsNullOrEmpty(status))
        {
            if (Enum.TryParse<ClaimStatus>(status, out var statusEnum))
            {
                query = query.Where(c => c.Status == statusEnum);
            }
        }

        if (insuranceCompanyId.HasValue)
        {
            query = query.Where(c => c.InsuranceCompanyId == insuranceCompanyId.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(c => c.SubmittedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(c => c.SubmittedAt <= toDate.Value);
        }

        return await query
            .OrderByDescending(c => c.SubmittedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<Claim>> GetClaimsForCheckerAsync(int pageNumber = 1, int pageSize = 10,
        string? status = null, Guid? insuranceCompanyId = null, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.Claims
            .Include(c => c.InsuranceCompany)
            .Include(c => c.MakerReview)
                .ThenInclude(r => r!.ReviewedByUser)
            .AsQueryable();

        // Checker can only see MakerSubmitted claims
        query = query.Where(c => c.Status == ClaimStatus.MakerSubmitted || c.Status == ClaimStatus.CheckerInProgress);

        if (!string.IsNullOrEmpty(status))
        {
            if (Enum.TryParse<ClaimStatus>(status, out var statusEnum))
            {
                query = query.Where(c => c.Status == statusEnum);
            }
        }

        if (insuranceCompanyId.HasValue)
        {
            query = query.Where(c => c.InsuranceCompanyId == insuranceCompanyId.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(c => c.SubmittedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(c => c.SubmittedAt <= toDate.Value);
        }

        return await query
            .OrderByDescending(c => c.SubmittedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetTotalCountAsync(string? status = null, Guid? insuranceCompanyId = null, 
        DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.Claims.AsQueryable();

        if (!string.IsNullOrEmpty(status))
        {
            if (Enum.TryParse<ClaimStatus>(status, out var statusEnum))
            {
                query = query.Where(c => c.Status == statusEnum);
            }
        }

        if (insuranceCompanyId.HasValue)
        {
            query = query.Where(c => c.InsuranceCompanyId == insuranceCompanyId.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(c => c.SubmittedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(c => c.SubmittedAt <= toDate.Value);
        }

        return await query.CountAsync();
    }

    public async Task<Claim?> LockClaimAsync(Guid claimId, Guid userId, string role)
    {
        var claim = await _context.Claims.FirstOrDefaultAsync(c => c.Id == claimId);
        
        if (claim == null)
            return null;

        // Check if claim is already locked
        if (claim.LockedByUserId != null && claim.LockedByUserId != userId)
            return null;

        // Check if claim is in appropriate state
        if (role == "Maker" && claim.Status != ClaimStatus.Pending && claim.Status != ClaimStatus.MakerInProgress)
            return null;

        if (role == "Checker" && claim.Status != ClaimStatus.MakerSubmitted && claim.Status != ClaimStatus.CheckerInProgress)
            return null;

        claim.LockedByUserId = userId;
        claim.LockedAt = DateTime.UtcNow;
        claim.Status = role == "Maker" ? ClaimStatus.MakerInProgress : ClaimStatus.CheckerInProgress;

        _context.Claims.Update(claim);
        await _context.SaveChangesAsync();

        return claim;
    }

    public async Task<bool> UnlockClaimAsync(Guid claimId, Guid userId)
    {
        var claim = await _context.Claims.FirstOrDefaultAsync(c => c.Id == claimId);
        
        if (claim == null || claim.LockedByUserId != userId)
            return false;

        claim.LockedByUserId = null;
        claim.LockedAt = null;

        _context.Claims.Update(claim);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task AddAsync(Claim claim)
    {
        await _context.Claims.AddAsync(claim);
    }

    public async Task UpdateAsync(Claim claim)
    {
        claim.UpdatedAt = DateTime.UtcNow;
        _context.Claims.Update(claim);
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }
}
