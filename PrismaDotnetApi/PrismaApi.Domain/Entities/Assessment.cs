using Microsoft.EntityFrameworkCore;
using PrismaApi.Domain.Constants;
using PrismaApi.Domain.Interfaces;

namespace PrismaApi.Domain.Entities;

public class Assessment : AuditableEntity, IBaseEntity<Guid>
{
    public required Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public required Guid ProjectId { get; set; }

    public bool IsCompleted { get; set; } = false;
    public Project? Project { get; set; }
    public ICollection<DecisionQualityAssessment> DecisionQualityAssessments { get; set; } = new List<DecisionQualityAssessment>();
    public static void OnModelConfiguring(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Assessment>(static entity =>
        {
            entity.ToTable("Assessments");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(DomainConstants.MaxShortStringLength);
            entity.HasOne(e => e.CreatedBy)
            .WithMany()
            .HasForeignKey(e => e.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.UpdatedBy)
                .WithMany()
                .HasForeignKey(e => e.UpdatedById)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(e => e.DecisionQualityAssessments)
                .WithOne(e => e.Assessment)
                .HasForeignKey(e => e.AssessmentId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
