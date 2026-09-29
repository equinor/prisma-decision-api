using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using PrismaApi.Domain.Constants;
using PrismaApi.Domain.Interfaces;

namespace PrismaApi.Domain.Entities;

public class BoardSheet : AuditableEntity, IBaseEntity<Guid>, IEntityHandlingPolicy
{
    public required Guid Id { get; set; }
    public required Guid ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public Project? Project { get; set; }
    [NotMapped]
    public TransferBehavior IsTransferable => TransferBehavior.Transferable;
    public static void OnModelConfiguring(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BoardSheet>(entity =>
        {
            entity.ToTable("BoardSheet");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(DomainConstants.MaxShortStringLength);

            entity.HasOne(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(e => e.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.UpdatedBy)
                .WithMany()
                .HasForeignKey(e => e.UpdatedById)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
