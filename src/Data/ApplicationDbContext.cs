using Microsoft.EntityFrameworkCore;
using InsureZenv2.src.Models;

namespace InsureZenv2.src.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<InsuranceCompany> InsuranceCompanies { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Claim> Claims { get; set; }
    public DbSet<ClaimReview> ClaimReviews { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // InsuranceCompany configuration
        modelBuilder.Entity<InsuranceCompany>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.ContactEmail).HasMaxLength(255);
            entity.Property(e => e.ContactPhone).HasMaxLength(20);
        });

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).HasMaxLength(255).IsRequired();
            entity.HasIndex(e => e.Username).IsUnique();
            entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.Role).HasMaxLength(50).IsRequired();
            
            entity.HasOne(e => e.InsuranceCompany)
                .WithMany(ic => ic.Users)
                .HasForeignKey(e => e.InsuranceCompanyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Claim configuration
        modelBuilder.Entity<Claim>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ClaimNumber).HasMaxLength(100).IsRequired();
            entity.HasIndex(e => e.ClaimNumber).IsUnique();
            entity.Property(e => e.PatientName).HasMaxLength(255).IsRequired();
            entity.Property(e => e.PatientId).HasMaxLength(100).IsRequired();
            entity.Property(e => e.ClaimAmount).HasPrecision(18, 2);
            entity.Property(e => e.ServiceDescription).IsRequired();
            entity.Property(e => e.ProviderName).HasMaxLength(255).IsRequired();
            entity.Property(e => e.ProviderCode).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Status).HasConversion<int>();
            
            // Concurrency control - one claim can only be locked to one user
            entity.HasIndex(e => new { e.Id, e.LockedByUserId });
            
            entity.HasOne(e => e.InsuranceCompany)
                .WithMany(ic => ic.Claims)
                .HasForeignKey(e => e.InsuranceCompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.LockedByUser)
                .WithMany()
                .HasForeignKey(e => e.LockedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.MakerReview)
                .WithOne(cr => cr.Claim)
                .HasForeignKey<ClaimReview>(cr => new { cr.ClaimId })
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ClaimReview configuration
        modelBuilder.Entity<ClaimReview>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Feedback).IsRequired();
            entity.Property(e => e.MakerRecommendation).HasConversion<int?>();
            entity.Property(e => e.CheckerDecision).HasConversion<int?>();
            
            entity.HasOne(e => e.ReviewedByUser)
                .WithMany()
                .HasForeignKey(e => e.ReviewedByUserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Composite index for tracking maker and checker reviews per claim
            entity.HasIndex(e => new { e.ClaimId, e.IsMakerReview });
        });

        // AuditLog configuration
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Action).HasMaxLength(255).IsRequired();
            entity.HasIndex(e => e.ClaimId);
            entity.HasIndex(e => e.CreatedAt);

            entity.HasOne(e => e.Claim)
                .WithMany()
                .HasForeignKey(e => e.ClaimId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
