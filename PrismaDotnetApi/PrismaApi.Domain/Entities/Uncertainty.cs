using Microsoft.EntityFrameworkCore;
using PrismaApi.Domain.Interfaces;

namespace PrismaApi.Domain.Entities;

public class Uncertainty : BaseEntity, IBaseEntity<Guid>
{
    public required Guid Id { get; set; }
    public required Guid IssueId { get; set; }
    public required Guid ProjectId { get; set; }
    public bool IsKey { get; set; } = true;
    public Issue? Issue { get; set; }
    public Project? Project { get; set; }
    public ICollection<Outcome> Outcomes { get; set; } = new List<Outcome>();
    public ICollection<DiscreteProbability> DiscreteProbabilities { get; set; } = new List<DiscreteProbability>();
    public static void OnModelConfiguring(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Uncertainty>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.Project)
                .WithMany()
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.NoAction); // Cascade path already exists via Projects -> Issues -> Uncertainties

            entity.HasMany(e => e.Outcomes)
                .WithOne(e => e.Uncertainty)
                .HasForeignKey(e => e.UncertaintyId)
                .OnDelete(DeleteBehavior.Cascade);
        
        });
    }
}
