using Microsoft.EntityFrameworkCore;

namespace PrismaApi.Domain.Entities;

public class DiscreteUtilityParentOption
{
    public required Guid DiscreteUtilityId { get; set; }
    public required Guid ParentOptionId { get; set; }

    public DiscreteUtility? DiscreteUtility { get; set; }
    public Option? ParentOption { get; set; }
    public static void OnModelConfiguring(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DiscreteUtilityParentOption>(entity =>
        {
            entity.HasKey(e => new { e.DiscreteUtilityId, e.ParentOptionId });

            entity.HasOne(e => e.DiscreteUtility)
                .WithMany(d => d.ParentOptions)
                .HasForeignKey(e => e.DiscreteUtilityId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ParentOption)
                .WithMany()
                .HasForeignKey(e => e.ParentOptionId)
                .OnDelete(DeleteBehavior.NoAction);
        });
    }
}
