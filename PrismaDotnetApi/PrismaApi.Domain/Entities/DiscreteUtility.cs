using Microsoft.EntityFrameworkCore;
using PrismaApi.Domain.Constants;
using PrismaApi.Domain.Interfaces;

namespace PrismaApi.Domain.Entities;

public class DiscreteUtility : BaseEntity, IBaseEntity<Guid>
{
    public required Guid Id { get; set; }
    public required Guid ValueMetricId { get; set; }
    public required Guid UtilityId { get; set; }
    public required Guid ProjectId { get; set; }
    public double? UtilityValue { get; set; }

    public ValueMetric? ValueMetric { get; set; }
    public Utility? Utility { get; set; }
    public Project? Project { get; set; }

    public ICollection<DiscreteUtilityParentOutcome> ParentOutcomes { get; set; } = new List<DiscreteUtilityParentOutcome>();
    public ICollection<DiscreteUtilityParentOption> ParentOptions { get; set; } = new List<DiscreteUtilityParentOption>();
    public static void OnModelConfiguring(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DiscreteUtility>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ValueMetricId);
            entity.HasIndex(e => e.UtilityId);
            entity.Property(e => e.UtilityValue).HasPrecision(DomainConstants.FloatPrecision);

            entity.HasOne(e => e.Project)
                .WithMany()
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.NoAction); // Cascade path already exists via Projects -> Issues -> Utilities -> DiscreteUtilities

            entity.HasOne(e => e.ValueMetric)
                .WithMany()
                .HasForeignKey(e => e.ValueMetricId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Utility)
                .WithMany(e => e.DiscreteUtilities)
                .HasForeignKey(e => e.UtilityId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.ParentOutcomes)
                .WithOne(e => e.DiscreteUtility)
                .HasForeignKey(e => e.DiscreteUtilityId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.ParentOptions)
                .WithOne(e => e.DiscreteUtility)
                .HasForeignKey(e => e.DiscreteUtilityId)
                .OnDelete(DeleteBehavior.Cascade);
        });        
    }
}
