using Microsoft.EntityFrameworkCore;
using PrismaApi.Domain.Constants;
using PrismaApi.Domain.Interfaces;

namespace PrismaApi.Domain.Entities;

public class DecisionQualityAssessment : AuditableEntity, IBaseEntity<Guid>
{
    public required Guid Id { get; set; }
    public int AppropriateFrame { get; set; }
    public int TradeOffAnalysis { get; set; }
    public int ReasoningCorrectness { get; set; }
    public int InformationReliability { get; set; }
    public int CommitmentToAction { get; set; }
    public string Comment { get; set; } = string.Empty;

    public int DoableAlternatives { get; set; }
    public required Guid AssessmentId { get; set; }
    public required Guid ProjectId { get; set; }
    public Assessment? Assessment { get; set; }
    public Project? Project { get; set; }
    public static void OnModelConfiguring(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DecisionQualityAssessment>(entity =>
        {
            entity.ToTable("DecisionQualityAssessments");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Comment).HasMaxLength(DomainConstants.MaxLongStringLength);

            entity.HasOne(e => e.Project)
                .WithMany()
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.NoAction); // Cascade path already exists via Projects -> Assessments -> DecisionQualityAssessments

            entity.HasOne(e => e.CreatedBy)
            .WithMany()
            .HasForeignKey(e => e.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.UpdatedBy)
                .WithMany()
                .HasForeignKey(e => e.UpdatedById)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
