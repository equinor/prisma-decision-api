using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrismaApi.Domain.Entities;

public class StrategyOption : IEntityHandlingPolicy
{
    public required Guid StrategyId { get; set; }
    public required Guid OptionId { get; set; }
    public required Guid ProjectId { get; set; }

    public Strategy? Strategy { get; set; }
    public Option? Option { get; set; }

    [NotMapped]
    public TransferBehavior IsTransferable => TransferBehavior.Transferable;
    public Project? Project { get; set; }
    public static void OnModelConfiguring(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StrategyOption>(entity =>
        {
            entity.HasKey(e => new { e.StrategyId, e.OptionId });

            entity.HasOne(e => e.Option)
                .WithMany(e => e.StrategyOptions)
                .HasForeignKey(e => e.OptionId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.Project)
                .WithMany()
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.NoAction); // Cascade path already exists via Projects -> Strategy -> StrategyOption
        });
    }
}
