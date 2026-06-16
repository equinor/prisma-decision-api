using Microsoft.EntityFrameworkCore;
using PrismaApi.Domain.Interfaces;

namespace PrismaApi.Domain.Entities;

public class RestrictionEntry : AuditableEntity, IBaseEntity<Guid>
{
    public required Guid Id { get; set; }
    public required Guid ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;

    public double RestrictionValue { get; set; }

    // parent option id
    public required Guid? ParentOptionId { get; set; }
    // parent outcome id
    public required Guid? ParentOutcomeId { get; set; }
    // child option id 
    public required Guid? ChildOptionId { get; set; }
    // child outcome id
    public required Guid? ChildOutcomeId { get; set; }
    public required Guid RestrictionTableId { get; set; }
    public Project? Project { get; set; } = default;
    public Outcome? ParentOutcome { get; set; } = default;
    public Outcome? ChildOutcome { get; set; } = default;
    public Option? ParentOption { get; set; } = default;
    public Option? ChildOption { get; set; } = default;
    public RestrictionTable? RestrictionTable { get; set; } = default;
    public static void OnModelConfiguring(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RestrictionEntry>(entity =>
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
            
            entity.HasOne(e => e.RestrictionTable)
                .WithMany()
                .HasForeignKey(e => e.RestrictionTableId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ParentOption)
                .WithMany()
                .HasForeignKey(e => e.ParentOptionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ChildOption)
                .WithMany()
                .HasForeignKey(e => e.ChildOptionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ParentOutcome)
                .WithMany()
                .HasForeignKey(e => e.ParentOutcomeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ChildOutcome)
                .WithMany()
                .HasForeignKey(e => e.ChildOutcomeId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}