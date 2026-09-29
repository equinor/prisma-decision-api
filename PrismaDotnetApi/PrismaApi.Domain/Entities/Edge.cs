using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using PrismaApi.Domain.Interfaces;

namespace PrismaApi.Domain.Entities;

public class Edge : BaseEntity, IBaseEntity<Guid>, IEntityHandlingPolicy
{
    public required Guid Id { get; set; }
    public required Guid TailId { get; set; }
    public required Guid HeadId { get; set; }
    public required Guid ProjectId { get; set; }

    public Project? Project { get; set; }
    public Node? TailNode { get; set; }
    public Node? HeadNode { get; set; }
    [NotMapped]
    public TransferBehavior IsTransferable => TransferBehavior.Transferable;
    public static void OnModelConfiguring(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Edge>(entity =>
        {
            entity.HasKey(e => e.Id);
        });
    }
}
