using Microsoft.EntityFrameworkCore;
using PrismaApi.Domain.Constants;
using PrismaApi.Domain.Interfaces;

namespace PrismaApi.Domain.Entities;

public class Strategy : AuditableEntity, IBaseEntity<Guid>
{
    public required Guid Id { get; set; }
    public required Guid ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Rationale { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string IconColor { get; set; } = string.Empty;

    public Project? Project { get; set; }
    public ICollection<StrategyOption> StrategyOptions { get; set; } = new List<StrategyOption>();
    public static void OnModelConfiguring(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Strategy>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(e => e.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.UpdatedBy)
                .WithMany()
                .HasForeignKey(e => e.UpdatedById)
                .OnDelete(DeleteBehavior.Restrict);
            entity.Property(e => e.Name).HasMaxLength(DomainConstants.MaxShortStringLength);
            entity.Property(e => e.Description).HasMaxLength(DomainConstants.MaxLongStringLength);
            entity.Property(e => e.Rationale).HasMaxLength(DomainConstants.MaxLongStringLength);

            entity.HasMany(e => e.StrategyOptions)
                .WithOne(e => e.Strategy)
                .HasForeignKey(e => e.StrategyId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
