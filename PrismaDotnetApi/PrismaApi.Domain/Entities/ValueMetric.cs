using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using PrismaApi.Domain.Constants;
using PrismaApi.Domain.Interfaces;

namespace PrismaApi.Domain.Entities;

public class ValueMetric : BaseEntity, IBaseEntity<Guid>, IEntityHandlingPolicy
{
    public required Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    [NotMapped]
    public TransferBehavior IsTransferable => TransferBehavior.NotTransferable;
    public static void OnModelConfiguring(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ValueMetric>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(DomainConstants.MaxShortStringLength);
        });
        modelBuilder.Entity<ValueMetric>().HasData(new ValueMetric
        {
            Id = DomainConstants.DefaultValueMetricId,
            Name = DomainConstants.DefaultValueMetricName,
            CreatedAt = new DateTimeOffset(new DateTime(2020, 1, 1, 1, 1, 1, 1, DateTimeKind.Utc).AddTicks(1), TimeSpan.Zero),
            UpdatedAt = new DateTimeOffset(new DateTime(2020, 1, 1, 1, 1, 1, 1, DateTimeKind.Utc).AddTicks(2), TimeSpan.Zero)
        });
    }
}
