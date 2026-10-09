using Microsoft.EntityFrameworkCore;

namespace TenderEvidenceChecker.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<AnalysisEntity> Analyses => Set<AnalysisEntity>();
    public DbSet<StoredFileEntity> Files => Set<StoredFileEntity>();
    public DbSet<PageTextEntity> Pages => Set<PageTextEntity>();
    public DbSet<RequirementEntity> Requirements => Set<RequirementEntity>();
    public DbSet<EvidenceMatchEntity> Matches => Set<EvidenceMatchEntity>();
    public DbSet<JobEntity> Jobs => Set<JobEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AnalysisEntity>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedNever();
            entity.Property(x => x.Label).HasMaxLength(200);
            entity.Property(x => x.Status).HasMaxLength(64);
            entity.HasIndex(x => x.CreatedAt);
        });

        modelBuilder.Entity<StoredFileEntity>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedNever();
            entity.Property(x => x.OriginalName).HasMaxLength(180);
            entity.Property(x => x.StoredName).HasMaxLength(80);
            entity.Property(x => x.Sha256).HasMaxLength(64);
            entity.HasOne(x => x.Analysis)
                .WithMany(x => x.Files)
                .HasForeignKey(x => x.AnalysisId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PageTextEntity>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedNever();
            entity.HasIndex(x => new { x.FileId, x.PageNumber }).IsUnique();
            entity.HasOne(x => x.File)
                .WithMany(x => x.Pages)
                .HasForeignKey(x => x.FileId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RequirementEntity>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedNever();
            entity.HasOne(x => x.Analysis)
                .WithMany(x => x.Requirements)
                .HasForeignKey(x => x.AnalysisId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<EvidenceMatchEntity>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedNever();
            entity.HasOne(x => x.Requirement)
                .WithMany(x => x.Matches)
                .HasForeignKey(x => x.RequirementId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<JobEntity>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedNever();
            entity.HasIndex(x => x.Status);
            entity.HasOne(x => x.Analysis)
                .WithMany(x => x.Jobs)
                .HasForeignKey(x => x.AnalysisId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
