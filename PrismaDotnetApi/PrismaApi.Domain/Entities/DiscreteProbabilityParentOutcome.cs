using Microsoft.EntityFrameworkCore;

namespace PrismaApi.Domain.Entities;

public class DiscreteProbabilityParentOutcome
{
    public required Guid DiscreteProbabilityId { get; set; }
    public required Guid ParentOutcomeId { get; set; }

    public DiscreteProbability? DiscreteProbability { get; set; }
    public Outcome? ParentOutcome { get; set; }
    public static void OnModelConfiguring(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DiscreteProbabilityParentOutcome>(entity =>
        {
            entity.HasKey(e => new { e.DiscreteProbabilityId, e.ParentOutcomeId });

            entity.HasOne(e => e.DiscreteProbability)
                .WithMany(d => d.ParentOutcomes)
                .HasForeignKey(e => e.DiscreteProbabilityId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ParentOutcome)
                .WithMany()
                .HasForeignKey(e => e.ParentOutcomeId)
                .OnDelete(DeleteBehavior.NoAction);
        });
    }
}
