using Microsoft.EntityFrameworkCore;
using PrismaApi.Domain.Interfaces;

namespace PrismaApi.Domain.Entities;

public class NodeStyle : BaseEntity, IBaseEntity<Guid>
{
    public required Guid Id { get; set; }
    public required Guid NodeId { get; set; }
    public double XPosition { get; set; }
    public double YPosition { get; set; }

    public Node? Node { get; set; }
    public static void OnModelConfiguring(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NodeStyle>(entity =>
        {
            entity.HasKey(e => e.Id);
        });
    }
}
