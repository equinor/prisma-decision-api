using Microsoft.EntityFrameworkCore;
using PrismaApi.Domain.Constants;
using PrismaApi.Domain.Interfaces;

namespace PrismaApi.Domain.Entities;

public class Option : BaseEntity, IBaseEntity<Guid>
{
    public required Guid Id { get; set; }
    public required Guid DecisionId { get; set; }
    public required Guid ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Utility { get; set; }

    public Decision? Decision { get; set; }
    public Project? Project { get; set; }
    public ICollection<StrategyOption> StrategyOptions { get; set; } = new List<StrategyOption>();
    public static void OnModelConfiguring(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Option>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(DomainConstants.MaxShortStringLength);

            entity.HasOne(e => e.Project)
                .WithMany()
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.NoAction); // Cascade path already exists via Projects -> Issues -> Decisions -> Options
        });
    }
}
