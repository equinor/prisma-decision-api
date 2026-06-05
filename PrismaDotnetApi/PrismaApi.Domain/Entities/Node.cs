using Microsoft.EntityFrameworkCore;
using PrismaApi.Domain.Constants;
using PrismaApi.Domain.Interfaces;

namespace PrismaApi.Domain.Entities;

public class Node : BaseEntity, IBaseEntity<Guid>
{
    public required Guid Id { get; set; }
    public required Guid ProjectId { get; set; }
    public required Guid IssueId { get; set; }
    public string Name { get; set; } = string.Empty;

    public Project? Project { get; set; }
    public Issue? Issue { get; set; }

    public ICollection<Edge> HeadEdges { get; set; } = new List<Edge>();
    public ICollection<Edge> TailEdges { get; set; } = new List<Edge>();
    public NodeStyle? NodeStyle { get; set; }
    public static void OnModelConfiguring(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Node>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(DomainConstants.MaxShortStringLength);

            entity.HasOne(e => e.NodeStyle)
                .WithOne(e => e.Node)
                .HasForeignKey<NodeStyle>(e => e.NodeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.HeadEdges)
                .WithOne(e => e.HeadNode)
                .HasForeignKey(e => e.HeadId)
                .OnDelete(DeleteBehavior.NoAction); // deleted in cleanup

            entity.HasMany(e => e.TailEdges)
                .WithOne(e => e.TailNode)
                .HasForeignKey(e => e.TailId)
                .OnDelete(DeleteBehavior.NoAction); // deleted in cleanup
        });
    }
}
