using Microsoft.EntityFrameworkCore;

namespace PrismaApi.Domain.Entities;

public class DiscreteProbabilityParentOption
{
    public required Guid DiscreteProbabilityId { get; set; }
    public required Guid ParentOptionId { get; set; }

    public DiscreteProbability? DiscreteProbability { get; set; }
    public Option? ParentOption { get; set; }
    public static void OnModelConfiguring(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DiscreteProbabilityParentOption>(entity =>
        {
            entity.HasKey(e => new { e.DiscreteProbabilityId, e.ParentOptionId });

            entity.HasOne(e => e.DiscreteProbability)
                .WithMany(d => d.ParentOptions)
                .HasForeignKey(e => e.DiscreteProbabilityId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ParentOption)
                .WithMany()
                .HasForeignKey(e => e.ParentOptionId)
                .OnDelete(DeleteBehavior.NoAction);
        });
    }
}
