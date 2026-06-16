using Microsoft.EntityFrameworkCore;
using PrismaApi.Domain.Constants;
using PrismaApi.Domain.Interfaces;

namespace PrismaApi.Domain.Entities;

public class Objective : AuditableEntity, IBaseEntity<Guid>
{
    public required Guid Id { get; set; }
    public required Guid ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Project? Project { get; set; }
    public static void OnModelConfiguring(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Objective>(entity =>
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
            entity.Property(e => e.Type).HasMaxLength(DomainConstants.MaxShortStringLength);
            entity.Property(e => e.Description).HasMaxLength(DomainConstants.MaxLongStringLength);
        });
    }
}
