using Microsoft.EntityFrameworkCore;
using PrismaApi.Domain.Constants;
using PrismaApi.Domain.Interfaces;

namespace PrismaApi.Domain.Entities;

public class Issue : AuditableEntity, IBaseEntity<Guid>
{
    public required Guid Id { get; set; }
    public required Guid ProjectId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Boundary { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Order { get; set; }

    public Project? Project { get; set; }
    public Node? Node { get; set; }
    public Decision? Decision { get; set; }
    public Uncertainty? Uncertainty { get; set; }
    public Utility? Utility { get; set; }
    public static void OnModelConfiguring(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Issue>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(DomainConstants.MaxShortStringLength);
            entity.Property(e => e.Description).HasMaxLength(DomainConstants.MaxLongStringLength);
            entity.Property(e => e.Type).HasMaxLength(DomainConstants.MaxShortStringLength);
            entity.Property(e => e.Boundary).HasMaxLength(DomainConstants.MaxShortStringLength);

            entity.HasOne(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(e => e.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.UpdatedBy)
                .WithMany()
                .HasForeignKey(e => e.UpdatedById)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.Node)
                .WithOne(e => e.Issue)
                .HasForeignKey<Node>(e => e.IssueId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Decision)
                .WithOne(e => e.Issue)
                .HasForeignKey<Decision>(e => e.IssueId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Uncertainty)
                .WithOne(e => e.Issue)
                .HasForeignKey<Uncertainty>(e => e.IssueId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Utility)
                .WithOne(e => e.Issue)
                .HasForeignKey<Utility>(e => e.IssueId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
