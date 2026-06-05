using Microsoft.EntityFrameworkCore;
using PrismaApi.Domain.Constants;
using PrismaApi.Domain.Interfaces;

namespace PrismaApi.Domain.Entities;

public class DiscreteProbability : BaseEntity, IBaseEntity<Guid>
{
    public required Guid Id { get; set; }
    public required Guid OutcomeId { get; set; }
    public required Guid UncertaintyId { get; set; }
    public required Guid ProjectId { get; set; }
    public double? Probability { get; set; }

    public Outcome? Outcome { get; set; }
    public Uncertainty? Uncertainty { get; set; }
    public Project? Project { get; set; }

    public ICollection<DiscreteProbabilityParentOutcome> ParentOutcomes { get; set; } = new List<DiscreteProbabilityParentOutcome>();
    public ICollection<DiscreteProbabilityParentOption> ParentOptions { get; set; } = new List<DiscreteProbabilityParentOption>();
    public static void OnModelConfiguring(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DiscreteProbability>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.OutcomeId);
            entity.HasIndex(e => e.UncertaintyId);
            entity.Property(e => e.Probability).HasPrecision(DomainConstants.FloatPrecision);

            entity.HasOne(e => e.Project)
                .WithMany()
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.NoAction); // Cascade path already exists via Projects -> Issues -> Uncertainties -> Outcomes -> DiscreteProbabilities

            entity.HasOne(e => e.Outcome)
                .WithMany()
                .HasForeignKey(e => e.OutcomeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Uncertainty)
                .WithMany(e => e.DiscreteProbabilities)
                .HasForeignKey(e => e.UncertaintyId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasMany(e => e.ParentOutcomes)
                .WithOne(e => e.DiscreteProbability)
                .HasForeignKey(e => e.DiscreteProbabilityId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.ParentOptions)
                .WithOne(e => e.DiscreteProbability)
                .HasForeignKey(e => e.DiscreteProbabilityId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
