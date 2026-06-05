using Microsoft.EntityFrameworkCore;

namespace PrismaApi.Domain.Entities;

public class DiscreteUtilityParentOutcome
{
    public required Guid DiscreteUtilityId { get; set; }
    public required Guid ParentOutcomeId { get; set; }

    public DiscreteUtility? DiscreteUtility { get; set; }
    public Outcome? ParentOutcome { get; set; }
    public static void OnModelConfiguring(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DiscreteUtilityParentOutcome>(entity =>
        {
            entity.HasKey(e => new { e.DiscreteUtilityId, e.ParentOutcomeId });

            entity.HasOne(e => e.DiscreteUtility)
                .WithMany(d => d.ParentOutcomes)
                .HasForeignKey(e => e.DiscreteUtilityId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ParentOutcome)
                .WithMany()
                .HasForeignKey(e => e.ParentOutcomeId)
                .OnDelete(DeleteBehavior.NoAction);
        });
    }
}
