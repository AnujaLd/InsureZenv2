using Microsoft.EntityFrameworkCore;
using InsureZenv2.src.Data;
using InsureZenv2.src.Models;

namespace InsureZenv2.src.Repositories;

public interface IClaimReviewRepository
{
    Task<ClaimReview?> GetByIdAsync(Guid id);
    Task<ClaimReview?> GetMakerReviewAsync(Guid claimId);
    Task<ClaimReview?> GetCheckerReviewAsync(Guid claimId);
    Task AddAsync(ClaimReview review);
    Task UpdateAsync(ClaimReview review);
    Task<bool> SaveChangesAsync();
}

public class ClaimReviewRepository : IClaimReviewRepository
{
    private readonly ApplicationDbContext _context;

    public ClaimReviewRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ClaimReview?> GetByIdAsync(Guid id)
    {
        return await _context.ClaimReviews
            .Include(cr => cr.ReviewedByUser)
            .FirstOrDefaultAsync(cr => cr.Id == id);
    }

    public async Task<ClaimReview?> GetMakerReviewAsync(Guid claimId)
    {
        return await _context.ClaimReviews
            .Include(cr => cr.ReviewedByUser)
            .FirstOrDefaultAsync(cr => cr.ClaimId == claimId && cr.IsMakerReview);
    }

    public async Task<ClaimReview?> GetCheckerReviewAsync(Guid claimId)
    {
        return await _context.ClaimReviews
            .Include(cr => cr.ReviewedByUser)
            .FirstOrDefaultAsync(cr => cr.ClaimId == claimId && !cr.IsMakerReview);
    }

    public async Task AddAsync(ClaimReview review)
    {
        await _context.ClaimReviews.AddAsync(review);
    }

    public async Task UpdateAsync(ClaimReview review)
    {
        review.UpdatedAt = DateTime.UtcNow;
        _context.ClaimReviews.Update(review);
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }
}
